
using Microsoft.EntityFrameworkCore;
using TTX.Core.Models;
using TTX.Core.Repositories;
using TTX.Infrastructure.Data;

public class VoteRepository(ApplicationDbContext context) : IVoteRepository
{
    public async Task RecordVote(Vote vote)
    {
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO votes (creator_id, value, time) VALUES ({vote.CreatorId}, {vote.Value}, {vote.Time})");
    }

    public async Task<Vote[]> GetAll(int creatorId, TimeStep step = TimeStep.Hour, DateTimeOffset? after = null)
    {
        after ??= DateTimeOffset.UtcNow.AddDays(-7);
        var interval = GetInterval(step);

        var sql = $@"
            SELECT
                votes.creator_id AS ""CreatorId"", 
                time_bucket_gapfill(
                    '{interval}', 
                    votes.time,
                    now(),
                    '{after.Value.UtcDateTime:yyyy-MM-dd HH:mm:ss}'
                ) AS ""Bucket"", 
                locf (last (votes.value, votes.time)) AS ""Value""
            FROM votes
            WHERE votes.creator_id = {creatorId}
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
            result.Add(new Vote
            {
                CreatorId = rows.GetInt32(0),
                Time = rows.GetDateTime(1),  // Maps "Bucket" to "Time"
                Value = rows.GetInt32(2)
            });
        }

        return [.. result];
    }

    public async Task<Dictionary<int, Vote[]>> GetAllFor(int[] creatorIds, TimeStep step = TimeStep.Hour, DateTimeOffset? after = null)
    {
        if (creatorIds.Length == 0)
            return [];

        after ??= DateTimeOffset.UtcNow.AddDays(-7);
        var interval = GetInterval(step);

        var creatorIdsStr = string.Join(", ", creatorIds);
        var sql = $@"
            SELECT
                votes.creator_id AS ""CreatorId"", 
                time_bucket_gapfill(
                    '{interval}', 
                    votes.time,
                    now(),
                    '{after.Value.UtcDateTime:yyyy-MM-dd HH:mm:ss}'
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

            result[creatorId].Add(new Vote
            {
                CreatorId = creatorId,
                Time = rows.GetDateTime(1),  // Maps "Bucket" to "Time"
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