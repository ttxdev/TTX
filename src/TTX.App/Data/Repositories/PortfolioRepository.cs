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
                locf(
                    last(p.value, p.time),
                    (SELECT p2.value FROM player_portfolios p2
                     WHERE p2.player_id = p.player_id AND p2.time < @start_time
                     ORDER BY p2.time DESC LIMIT 1)
                ) AS ""Value""
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
            if (rows.IsDBNull(2)) continue;
            int playerId = rows.GetInt32(0);
            DateTime bucketTime = rows.GetDateTime(1);
            double value = rows.GetDouble(2);

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
        string interval = step.ToTimescaleString();
        Dictionary<int, List<Vote>> result = creators.ToDictionary(
            c => c.Id.Value,
            _ => new List<Vote>()
        );

        DbConnection connection = _dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        foreach (Creator creator in creators)
        {
            DateTimeOffset endTime = creator.StreamStatus.IsLive ? now : creator.StreamStatus.EndedAt;
            DateTimeOffset startTime = endTime - before;

            using DbCommand command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT
                    time_bucket_gapfill(
                        @interval::interval,
                        v.time,
                        @start_time,
                        @end_time
                    ) AS ""Bucket"",
                    locf(
                        last(v.value, v.time),
                        (SELECT v2.value FROM votes v2
                         WHERE v2.creator_id = @id AND v2.time < @start_time
                         ORDER BY v2.time DESC LIMIT 1)
                    ) AS ""Value""
                FROM votes v
                WHERE v.creator_id = @id
                    AND v.time >= @start_time
                    AND v.time <= @end_time
                GROUP BY ""Bucket""
                ORDER BY ""Bucket"" ASC";
            command.Parameters.Add(new NpgsqlParameter("id", creator.Id.Value));
            command.Parameters.Add(new NpgsqlParameter("interval", interval));
            command.Parameters.Add(new NpgsqlParameter("start_time", startTime));
            command.Parameters.Add(new NpgsqlParameter("end_time", endTime));

            using DbDataReader rows = await command.ExecuteReaderAsync();
            List<Vote> list = result[creator.Id.Value];

            while (await rows.ReadAsync())
            {
                if (rows.IsDBNull(1)) continue;
                DateTime bucketTime = rows.GetDateTime(0);
                double value = rows.GetDouble(1);

                list.Add(new Vote
                {
                    Creator = creator,
                    CreatorId = creator.Id,
                    Time = bucketTime,
                    Value = value
                });
            }
        }

        return result.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
    }
}
