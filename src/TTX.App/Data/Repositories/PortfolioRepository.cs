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

    public async Task<Vote[]> FindCreatorHistory(
        Creator creator,
        TimeStep step,
        TimeSpan before
    )
    {
        // 1. Connection Management: Open explicitly via EF
        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        string intervalStr = step.ToTimescaleString();
        DateTimeOffset endTime = creator.StreamStatus.EndedAt;
        DateTimeOffset startTime = endTime - before;

        using DbCommand command = connection.CreateCommand();

        command.CommandText = $@"
            WITH initial_val AS (
                -- Find the last value immediately BEFORE the window starts
                SELECT
                    @id AS creator_id,
                    @start_time::timestamptz AS time,
                    value
                FROM votes
                WHERE creator_id = @id AND time < @start_time
                ORDER BY time DESC
                LIMIT 1
            ),
            combined_data AS (
                -- Combine the 'lookback' value with the actual range data
                SELECT creator_id, time, value FROM votes
                WHERE creator_id = @id
                  AND time >= @start_time
                  AND time <= @end_time
                UNION ALL
                SELECT creator_id, time, value FROM initial_val
            )
            SELECT
                c.creator_id AS ""CreatorId"",
                time_bucket_gapfill(
                    @interval::interval,
                    c.time,
                    @start_time,
                    @end_time
                ) AS ""Bucket"",
                locf(last(c.value, c.time)) AS ""Value""
            FROM combined_data c
            WHERE c.creator_id = @id -- Redundant but safe
            GROUP BY ""CreatorId"", ""Bucket""
            ORDER BY ""Bucket"" ASC";

        command.Parameters.Add(new NpgsqlParameter("id", creator.Id.Value));
        command.Parameters.Add(new NpgsqlParameter("interval", intervalStr));
        command.Parameters.Add(new NpgsqlParameter("start_time", startTime));
        command.Parameters.Add(new NpgsqlParameter("end_time", endTime));

        using DbDataReader rows = await command.ExecuteReaderAsync();

        List<Vote> result = [];
        while (await rows.ReadAsync())
        {
            DateTime bucketTime = rows.GetDateTime(1);

            double? value = rows.IsDBNull(2) ? null : rows.GetDouble(2);

            if (value is null || !creator.StreamStatus.IsLive && bucketTime > creator.StreamStatus.EndedAt.UtcDateTime)
            {
                continue;
            }

            result.Add(new Vote
            {
                Creator = creator,
                CreatorId = creator.Id,
                Time = bucketTime,
                Value = value
            });
        }

        return [.. result];
    }

    public async Task<Dictionary<int, Vote[]>> IndexCreatorHistory(
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

        DateTimeOffset globalEndTime = creators.Min(c => c.StreamStatus.StartedAt);
        DateTimeOffset globalStartTime = globalEndTime - before;
        Dictionary<int, List<Vote>> result = creators.ToDictionary(
            c => c.Id.Value,
            _ => new List<Vote>()
        );

        using DbCommand command = _dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = $@"
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
        command.Parameters.Add(new NpgsqlParameter("ids", creatorIds));
        command.Parameters.Add(new NpgsqlParameter("interval", interval));
        command.Parameters.Add(new NpgsqlParameter("start_time", globalStartTime));
        command.Parameters.Add(new NpgsqlParameter("end_time", globalEndTime));

        if (command.Connection!.State != ConnectionState.Open)
        {
            await command.Connection.OpenAsync();
        }

        using DbDataReader rows = await command.ExecuteReaderAsync();

        while (await rows.ReadAsync())
        {
            int creatorId = rows.GetInt32(0);
            DateTime bucketTime = rows.GetDateTime(1);
            double value = rows.IsDBNull(2) ? Creator.MinValue : rows.GetDouble(2);

            if (!creatorLookup.TryGetValue(creatorId, out var creator)) continue;

            if (!creator.StreamStatus.IsLive && bucketTime > creator.StreamStatus.EndedAt.UtcDateTime)
            {
                continue;
            }

            if (result.TryGetValue(creatorId, out var list))
            {
                list.Add(new Vote
                {
                    Creator = creator,
                    CreatorId = creator.Id,
                    Time = bucketTime,
                    Value = value
                });
            }
        }

        // Convert Lists to Arrays for the return type
        return result.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
    }
}
