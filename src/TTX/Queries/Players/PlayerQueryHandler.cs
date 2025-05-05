using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TTX.Infrastructure.Data;
using TTX.Models;

namespace TTX.Queries.Players
{
    public class PlayerQueryHandler(ApplicationDbContext context)
    {
        protected readonly ApplicationDbContext Context = context;

        protected async Task<Dictionary<int, PortfolioSnapshot[]>> GetHistoryFor(Player[] players, TimeStep step,
            DateTimeOffset after, CancellationToken ct)
        {
            if (players.Length == 0)
            {
                return [];
            }

            string interval = GetInterval(step);

            int[] playerIds = players.Select(c => c.Id.Value).ToArray();
            string playerIdsStr = string.Join(", ", playerIds);
            string sql = $@"
            SELECT
                player_portfolios.player_id AS ""PlayerId"",
                time_bucket_gapfill(
                    '{interval}',
                    player_portfolios.time,
                    '{after.UtcDateTime:yyyy-MM-dd HH:mm:ss}'::timestamptz,
                    now()
                ) AS ""Bucket"",
                locf (last (player_portfolios.value, player_portfolios.time)) AS ""Value""
            FROM player_portfolios
            WHERE player_portfolios.player_id IN ({playerIdsStr})
            GROUP BY ""PlayerId"", ""Bucket""
            ORDER BY ""Bucket"" ASC";

            using DbCommand command = Context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            if (command.Connection!.State != ConnectionState.Open)
            {
                await command.Connection.OpenAsync(ct);
            }

            using DbDataReader rows = await command.ExecuteReaderAsync(ct);
            Dictionary<int, List<PortfolioSnapshot>> result = new();

            while (await rows.ReadAsync(ct))
            {
                int playerId = rows.GetInt32(0);
                if (!result.ContainsKey(playerId))
                {
                    result[playerId] = [];
                }

                // TODO(dylhack): update the query so we don't have to check out of window timestamps
                DateTime time = rows.GetDateTime(1); // Maps "Bucket" to "Time"
                if (time < after)
                {
                    continue;
                }

                long value = rows.IsDBNull(2) ? Player.MinPortfolio : rows.GetInt64(2);
                Player player = players.First(c => c.Id.Value == playerId);
                result[playerId]
                    .Add(new PortfolioSnapshot { Player = player, PlayerId = player.Id, Time = time, Value = value });
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
                _ => throw new NotImplementedException()
            };
        }
    }
}