using Microsoft.EntityFrameworkCore;
using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Infrastructure.Data.Repositories;

public class CreatorRepository(ApplicationDbContext context) : RepositoryBase<Creator>(context), ICreatorRepository
{
    private readonly ApplicationDbContext context = context;

    public Task<Creator[]> GetAll() => DbSet.ToArrayAsync();
    public Task<Creator[]> GetAllByIds(int[] ids) => DbSet.Where(c => ids.Contains(c.Id)).ToArrayAsync();
    public async Task<Creator?> GetDetails(string slug, HistoryParams hParams)
    {
        var creator = await DbSet
                      .Include(c => c.Transactions.OrderByDescending(t => t.CreatedAt))
                      .ThenInclude(t => t.User)
                      .FirstOrDefaultAsync(c => c.Slug == slug);
        if (creator is null) return null;

        var history = await GetHistoryFor([creator.Id], hParams.Step, hParams.After);
        if (history.TryGetValue(creator.Id, out Vote[]? value))
            creator.History = [.. value];

        return creator;
    }
    public Task<Creator[]> GetAllAbove(long value) => DbSet.Where(c => c.Value > value).ToArrayAsync();
    public Task<long> GetValueSum() => DbSet.SumAsync(c => c.Value);
    public async Task<Creator?> UpdateStreamInfo(int id, StreamStatus status)
    {
        var creator = DbSet.FirstOrDefault(c => c.Id == id);
        if (creator is null) return null;

        creator.StreamStatus = status;
        Update(creator);
        await SaveChanges();

        return creator;
    }

    public Task<Creator?> Find(int creatorId) => DbSet.FirstOrDefaultAsync(c => c.Id == creatorId);

    public async Task<Pagination<Creator>> GetPaginated(int page, int limit, HistoryParams hParams, Order[]? order = null, Search? search = null)
    {
        var query = DbSet.AsQueryable();

        if (search is not null)
            query = query.Where(e => EF.Property<string>(e, search.By)!.ToLower().Contains(search.Value.ToLower()));

        if (order?.Length > 0)
        {
            // NOTE(dylhack): This is a hack to get around the fact that EF Core doesn't support
            //                ordering by a nested property. We should probably fix this in the future.
            IOrderedQueryable<Creator> orderedQuery = order[0].By == "IsLive"
                    ? (order[0].Dir == OrderDirection.Ascending ? query.OrderBy(e => e.StreamStatus.IsLive) : query.OrderByDescending(e => e.StreamStatus.IsLive))
                    : (order[0].Dir == OrderDirection.Ascending ? query.OrderBy(e => EF.Property<object>(e, order[0].By!)) : query.OrderByDescending(e => EF.Property<object>(e, order[0].By!)));

            for (int i = 1; i < order.Length; i++)
            {
                var o = order[i];
                orderedQuery = o.By == "IsLive"
                    ? (o.Dir == OrderDirection.Ascending ? orderedQuery.ThenBy(e => e.StreamStatus.IsLive) : orderedQuery.ThenByDescending(e => e.StreamStatus.IsLive))
                    : (o.Dir == OrderDirection.Ascending ? orderedQuery.ThenBy(e => EF.Property<object>(e, o.By!)) : orderedQuery.ThenByDescending(e => EF.Property<object>(e, o.By!)));
            }

            query = orderedQuery;
        }

        var total = await query.CountAsync();
        var creators = await query.Skip((page - 1) * limit).Take(limit).ToArrayAsync();
        var history = await GetHistoryFor([.. creators.Select(c => c.Id)], hParams.Step, hParams.After);

        return new Pagination<Creator>
        {
            Total = total,
            Data = [.. creators.Select(c =>
            {
                if (history.TryGetValue(c.Id, out Vote[]? value))
                    c.History = [.. value];

                return c;
            })],
        };
    }

    public async Task<int?> GetId(string slug)
    {
        return await DbSet.Where(c => c.Slug == slug).Select(c => c.Id).FirstOrDefaultAsync();
    }

    public Task<Creator?> FindBySlug(string slug)
    {
        return DbSet.FirstOrDefaultAsync(c => c.Slug == slug);
    }

    public async Task<Vote[]?> PullLatestHistory(string slug, HistoryParams history)
    {
        var creator = await FindBySlug(slug);
        if (creator is null)
            return null;

        var interval = GetInterval(history.Step);
        var sql = $@"
            SELECT
                votes.creator_id AS ""CreatorId"", 
                time_bucket(
                    '{interval}', 
                    votes.time
                ) AS ""Bucket"", 
                last (votes.value, votes.time) AS ""Value""
            FROM votes
            WHERE votes.creator_id = {creator.Id}
                AND votes.time >= '{history.After.UtcDateTime:yyyy-MM-dd HH:mm:ss}'::timestamptz
                AND votes.time <= now()
            GROUP BY ""CreatorId"", ""Bucket""
            ORDER BY ""Bucket"" ASC";

        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;

        if (command.Connection!.State != System.Data.ConnectionState.Open)
            await command.Connection.OpenAsync();

        using var rows = await command.ExecuteReaderAsync();
        var result = new List<Vote>();

        while (await rows.ReadAsync())
        {
            // TODO(dylhack): update the query so we don't have to check out of window timestamps
            var time = rows.GetDateTime(1);  // Maps "Bucket" to "Time"
            if (time < history.After)
                continue;

            result.Add(new Vote
            {
                CreatorId = rows.GetInt32(0),
                Time = time,
                Value = rows.GetInt32(2)
            });
        }

        return [.. result];
    }

    public async Task RecordValue(Creator creator, Vote vote)
    {
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO votes (creator_id, value, time) VALUES ({vote.CreatorId}, {vote.Value}, {vote.Time})");
        Update(creator);
        await SaveChanges();
    }

    private async Task<Dictionary<int, Vote[]>> GetHistoryFor(int[] creatorIds, TimeStep step, DateTimeOffset after)
    {
        if (creatorIds.Length == 0)
            return [];

        var interval = GetInterval(step);

        var creatorIdsStr = string.Join(", ", creatorIds);
        var sql = $@"
            SELECT
                votes.creator_id AS ""CreatorId"", 
                time_bucket_gapfill(
                    '{interval}', 
                    votes.time,
                    '{after.UtcDateTime:yyyy-MM-dd HH:mm:ss}'::timestamptz,
                    now()
                ) AS ""Bucket"", 
                locf (last (votes.value, votes.time)) AS ""Value""
            FROM votes
            WHERE votes.creator_id IN ({creatorIdsStr})
            GROUP BY ""CreatorId"", ""Bucket""
            ORDER BY ""Bucket"" ASC";

        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;

        if (command.Connection!.State != System.Data.ConnectionState.Open)
            await command.Connection.OpenAsync();

        using var rows = await command.ExecuteReaderAsync();
        var result = new Dictionary<int, List<Vote>>();

        while (await rows.ReadAsync())
        {
            var creatorId = rows.GetInt32(0);
            if (!result.ContainsKey(creatorId))
                result[creatorId] = [];
            
            // TODO(dylhack): update the query so we don't have to check out of window timestamps
            var time = rows.GetDateTime(1);  // Maps "Bucket" to "Time"
            if (time < after)
                continue;

            result[creatorId].Add(new Vote
            {
                CreatorId = creatorId,
                Time = time,
                Value = rows.GetInt32(2)
            });
        }

        return result.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
    }

    private static string GetInterval(TimeStep step)
    {
        return step switch
        {
            TimeStep.Minute => "1 minute",
            TimeStep.FiveMinute => "5 minute",
            TimeStep.FifteenMinute => "15 minute",
            TimeStep.ThirtyMinute => "30 minute",
            TimeStep.Hour => "1 hour",
            TimeStep.Day => "1 day",
            TimeStep.Week => "1 week",
            TimeStep.Month => "1 month",
            _ => "1 hour"
        };
    }
}
