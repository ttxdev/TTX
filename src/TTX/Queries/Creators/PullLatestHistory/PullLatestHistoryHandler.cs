using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Models;

namespace TTX.Queries.Creators.PullLatestHistory
{
    public class PullLatestHistoryHandler(ApplicationDbContext context) : IQueryHandler<PullLatestHistoryQuery, Vote[]>
    {
        public async Task<Vote[]> Handle(PullLatestHistoryQuery request, CancellationToken ct = default)
        {
            Creator? creator = await context.Creators.SingleOrDefaultAsync(c => c.Slug == request.CreatorSlug, ct);
            if (creator is null)
            {
                throw new CreatorNotFoundException();
            }

            string interval = GetInterval(request.Step);
            string sql = $@"
            SELECT
                votes.creator_id AS ""CreatorId"", 
                time_bucket(
                    '{interval}', 
                    votes.time
                ) AS ""Bucket"", 
                last (votes.value, votes.time) AS ""Value""
            FROM votes
            WHERE votes.creator_id = {creator.Id}
                AND votes.time >= '{request.After.UtcDateTime:yyyy-MM-dd HH:mm:ss}'::timestamptz
                AND votes.time <= now()
            GROUP BY ""CreatorId"", ""Bucket""
            ORDER BY ""Bucket"" ASC";

            using DbCommand command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            if (command.Connection!.State != ConnectionState.Open)
            {
                await command.Connection.OpenAsync(ct);
            }

            using DbDataReader rows = await command.ExecuteReaderAsync();
            List<Vote> result = new();

            while (await rows.ReadAsync(ct))
            {
                int value = rows.IsDBNull(2) ? Creator.MinValue : rows.GetInt32(2);
                result.Add(new Vote
                {
                    Creator = creator,
                    CreatorId = creator.Id,
                    Time = rows.GetDateTime(1), // Maps "Bucket" to "Time"
                    Value = value
                });
            }

            return [.. result];
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
                _ => throw new NotImplementedException()
            };
        }
    }
}