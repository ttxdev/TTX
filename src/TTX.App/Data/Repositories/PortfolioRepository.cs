using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
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
        TimeSpan before
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

        Dictionary<int, Player> playerLookup = players.ToDictionary(p => p.Id.Value);
        int[] playerIds = [.. playerLookup.Keys];
        string interval = step.ToTimescaleString();

        DateTimeOffset endTime = DateTimeOffset.UtcNow;
        DateTimeOffset startTime = endTime - before;

        string sql = $@"
            SELECT
                p.player_id AS ""PlayerId"",
                time_bucket_gapfill(
                    @interval::interval,
                    p.time,
                    @start_time,
                    @end_time
                ) AS ""Bucket"",
                locf(last(p.value, p.time)) AS ""Value""
            FROM player_portfolios p
            WHERE p.player_id = ANY(@ids)
                AND p.time >= @start_time
                AND p.time <= @end_time
            GROUP BY ""PlayerId"", ""Bucket""
            ORDER BY ""Bucket"" ASC";

        using DbCommand command = _dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add(new NpgsqlParameter("ids", playerIds));
        command.Parameters.Add(new NpgsqlParameter("interval", interval));
        command.Parameters.Add(new NpgsqlParameter("start_time", startTime));
        command.Parameters.Add(new NpgsqlParameter("end_time", endTime));

        if (command.Connection!.State != ConnectionState.Open)
        {
            await command.Connection.OpenAsync();
        }

        using DbDataReader rows = await command.ExecuteReaderAsync();
        Dictionary<int, List<PortfolioSnapshot>> result = [];

        while (await rows.ReadAsync())
        {
            int playerId = rows.GetInt32(0);
            DateTime bucketTime = rows.GetDateTime(1);
            double value = rows.IsDBNull(2) ? Player.MinPortfolio : rows.GetDouble(2);

            if (!playerLookup.TryGetValue(playerId, out var player)) continue;

            if (!result.TryGetValue(playerId, out var list))
            {
                list = [];
                result[playerId] = list;
            }

            list.Add(new PortfolioSnapshot
            {
                Player = player,
                PlayerId = player.Id,
                Time = bucketTime,
                Value = value
            });
        }

        return result.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray());
    }


    public async Task<Dictionary<int, Vote[]>> GetHistoryFor(
        IEnumerable<Creator> creators,
        TimeStep step,
        TimeSpan before
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
        DateTimeOffset globalStartTime = globalEndTime - before;

        using DbCommand command = _dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = """
            SELECT
                sub.creator_id AS "CreatorId",
                time_bucket_gapfill(
                    @interval::interval,
                    sub.time,
                    @start_time,
                    @end_time
                ) AS "Bucket",
                locf(last(sub.value, sub.time)) AS "Value"
            FROM (
                SELECT creator_id, time, value
                FROM votes
                WHERE creator_id = ANY(@ids)
                    AND time >= @start_time
                    AND time <= @end_time

                UNION ALL

                SELECT DISTINCT ON (creator_id)
                    creator_id,
                    @start_time AS time,
                    value
                FROM votes
                WHERE creator_id = ANY(@ids)
                    AND time < @start_time
                ORDER BY creator_id, time DESC
            ) sub
            GROUP BY "CreatorId", "Bucket"
            ORDER BY "Bucket" ASC
            """;

        command.Parameters.Add(new NpgsqlParameter("ids", creatorIds));
        command.Parameters.Add(new NpgsqlParameter("interval", interval));
        command.Parameters.Add(new NpgsqlParameter("start_time", globalStartTime));
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
            double value = rows.IsDBNull(2) ? Creator.MinValue : rows.GetDouble(2);

            if (!creatorLookup.TryGetValue(creatorId, out var creator))
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
