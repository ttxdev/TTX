using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
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

        DateTimeOffset now = DateTimeOffset.UtcNow;
        Dictionary<int, Creator> creatorLookup = creators.ToDictionary(c => c.Id.Value);
        int[] creatorIds = [.. creatorLookup.Keys];
        string interval = step.ToTimescaleString();

        DateTimeOffset globalEndTime = creators.Max(c =>
            c.StreamStatus.IsLive ? now : (c.StreamStatus.EndedAt ?? now));

        string sql = $@"
                SELECT
                    v.creator_id AS ""CreatorId"",
                    time_bucket_gapfill(
                        @interval::interval,
                        v.time,
                        @start_time,
                        @end_time
                    ) AS ""Bucket"",
                    locf(last(v.value, v.time)) AS ""Value""
                FROM votes v
                WHERE v.creator_id = ANY(@ids)
                    AND v.time >= @start_time
                    AND v.time <= @end_time
                GROUP BY ""CreatorId"", ""Bucket""
                ORDER BY ""Bucket"" ASC";

        using DbCommand command = _dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add(new NpgsqlParameter("ids", creatorIds));
        command.Parameters.Add(new NpgsqlParameter("interval", interval));
        command.Parameters.Add(new NpgsqlParameter("start_time", after));
        command.Parameters.Add(new NpgsqlParameter("end_time", globalEndTime));

        if (command.Connection!.State != ConnectionState.Open)
        {
            await command.Connection.OpenAsync();
        }

        using DbDataReader rows = await command.ExecuteReaderAsync();
        Dictionary<int, List<Vote>> result = [];

        while (await rows.ReadAsync())
        {
            int creatorId = rows.GetInt32(0);
            DateTime bucketTime = rows.GetDateTime(1);
            long value = rows.IsDBNull(2) ? Creator.MinValue : rows.GetInt64(2);

            if (!creatorLookup.TryGetValue(creatorId, out var creator)) continue;

            if (!creator.StreamStatus.IsLive &&
                creator.StreamStatus.EndedAt.HasValue &&
                bucketTime > creator.StreamStatus.EndedAt.Value.UtcDateTime)
            {
                continue;
            }

            if (!result.TryGetValue(creatorId, out var list))
            {
                list = [];
                result[creatorId] = list;
            }

            list.Add(new Vote
            {
                Creator = creator,
                CreatorId = creator.Id,
                Time = bucketTime,
                Value = value
            });
        }

        return result.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
    }
}
