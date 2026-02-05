using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TTX.App.Dto.Portfolio;
using TTX.Domain.Models;

namespace TTX.App.Data.Repositories;

public sealed class PortfolioRepository(ApplicationDbContext _dbContext)
{
    public Task StoreVote(Vote vote)
    {
        return _dbContext.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO votes (creator_id, value, time) VALUES ({vote.CreatorId.Value}, {vote.Value.Value}, {vote.Time})");
    }

    public Task StoreSnapshot(PortfolioSnapshot snapshot)
    {
        return _dbContext.Database.ExecuteSqlAsync($"INSERT INTO player_portfolios (player_id, value, time) VALUES ({snapshot.PlayerId.Value}, {snapshot.Value}, {snapshot.Time})");
    }

    public async Task<Dictionary<int, PortfolioSnapshot[]>> GetHistoryFor(
        IEnumerable<Player> players,
        TimeStep step,
        DateTimeOffset after
    )
    {
        if (!players.Any())
        {
            return [];
        }

        if (!_dbContext.Database.IsNpgsql())
        {
            throw new NotSupportedException("Only PostgreSQL is supported");
        }

        string interval = step.ToTimescaleString();
        int[] playerIds = [.. players.Select(c => c.Id.Value)];
        string playerIdsStr = string.Join(", ", playerIds);
        using DbCommand command = _dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = $@"
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

        if (command.Connection!.State != ConnectionState.Open)
        {
            await command.Connection.OpenAsync();
        }

        using DbDataReader rows = await command.ExecuteReaderAsync();
        Dictionary<int, List<PortfolioSnapshot>> result = [];

        while (await rows.ReadAsync())
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
            Player player = players.First(p => p.Id.Value == playerId);
            result[playerId]
                .Add(new PortfolioSnapshot { Player = player, PlayerId = player.Id, Time = time, Value = value });
        }

        return result.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray());
    }


    public async Task<Dictionary<int, Vote[]>> GetHistoryFor(
        IEnumerable<Creator> creators,
        TimeStep step,
        DateTimeOffset after
    )
    {
        if (!creators.Any())
        {
            return [];
        }

        if (!_dbContext.Database.IsNpgsql())
        {
            throw new NotSupportedException("Only PostgreSQL is supported");
        }

        string interval = step.ToTimescaleString();

        int[] creatorIds = [.. creators.Select(c => c.Id.Value)];
        string creatorIdsStr = string.Join(", ", creatorIds);
        string sql = $@"
            WITH creator_bounds AS (
                SELECT
                    id,
                    CASE
                        WHEN stream_is_live = true THEN now()
                        ELSE COALESCE(stream_ended_at, now())
                    END as last_live
                FROM creators
                WHERE id = ANY(@ids)
            )
            SELECT
                v.creator_id AS ""CreatorId"",
                time_bucket_gapfill(
                    @interval,
                    v.time,
                    @start_time,
                    cb.last_live
                ) AS ""Bucket"",
                locf(last(v.value, v.time)) AS ""Value""
            FROM votes v
            JOIN creator_bounds cb ON v.creator_id = cb.id
            WHERE v.creator_id = ANY(@ids)
                AND v.time >= @start_time
                AND v.time <= cb.last_live
            GROUP BY ""CreatorId"", ""Bucket"", cb.last_live
            ORDER BY ""Bucket"" ASC";

        using DbCommand command = _dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;

        DbParameter pIds = command.CreateParameter();
        pIds.ParameterName = "ids";
        pIds.Value = creatorIds;
        command.Parameters.Add(pIds);

        DbParameter pInterval = command.CreateParameter();
        pInterval.ParameterName = "interval";
        pInterval.Value = TimeSpan.Parse(interval);
        command.Parameters.Add(pInterval);

        DbParameter pStart = command.CreateParameter();
        pStart.ParameterName = "start_time";
        pStart.Value = after;
        command.Parameters.Add(pStart);

        if (command.Connection!.State != ConnectionState.Open)
        {
            await command.Connection.OpenAsync();
        }

        using DbDataReader rows = await command.ExecuteReaderAsync();
        Dictionary<int, List<Vote>> result = [];

        while (await rows.ReadAsync())
        {
            int creatorId = rows.GetInt32(0);
            DateTime time = rows.GetDateTime(1);

            // locf might return null if there's no data before the window
            long value = rows.IsDBNull(2) ? Creator.MinValue : rows.GetInt64(2);

            if (!result.TryGetValue(creatorId, out var list))
            {
                list ??= [];
                result[creatorId] = list;
            }

            Creator creator = creators.First(c => c.Id.Value == creatorId);
            list.Add(new Vote
            {
                Creator = creator,
                CreatorId = creator.Id,
                Time = time,
                Value = value
            });
        }

        return result.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
    }
}
