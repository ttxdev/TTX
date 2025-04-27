using Microsoft.EntityFrameworkCore;
using TTX.Infrastructure.Data;
using TTX.Models;

namespace TTX.Queries.Creators;

public class CreatorQueryHandler(ApplicationDbContext context)
{
    protected readonly ApplicationDbContext Context = context;

    protected async Task<Dictionary<int, Vote[]>> GetHistoryFor(Creator[] creators, TimeStep step, DateTimeOffset after, CancellationToken ct)
    {
        if (creators.Length == 0)
            return [];

        var interval = GetInterval(step);

        var creatorIds = creators.Select(c => c.Id.Value).ToArray();
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

        using var command = Context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;

        if (command.Connection!.State != System.Data.ConnectionState.Open)
            await command.Connection.OpenAsync(ct);

        using var rows = await command.ExecuteReaderAsync(ct);
        var result = new Dictionary<int, List<Vote>>();

        while (await rows.ReadAsync(ct))
        {
            var creatorId = rows.GetInt32(0);
            if (!result.ContainsKey(creatorId))
                result[creatorId] = [];

            // TODO(dylhack): update the query so we don't have to check out of window timestamps
            var time = rows.GetDateTime(1);  // Maps "Bucket" to "Time"
            if (time < after)
                continue;

            var value = rows.IsDBNull(2) ? Creator.MinValue : rows.GetInt32(2);
            Creator creator = creators.First(c => c.Id.Value == creatorId);
            result[creatorId].Add(new Vote
            {
                Creator = creator,
                CreatorId = creator.Id,
                Time = time,
                Value = value
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
            _ => throw new NotImplementedException(),
        };
    }
}
