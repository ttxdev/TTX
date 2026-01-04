using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using TTX.App.Dto.Pagination;
using TTX.App.Dto.Portfolio;
using TTX.App.Repositories;
using TTX.App.Services.Players;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;

namespace TTX.Infrastructure.Data.Repositories;

public sealed class PlayerRepository(ApplicationDbContext _dbContext, PortfolioRepository _portfolioRepository) : IPlayerRepository
{
    public async Task<IndexResult<Player>> Index(int page, int limit, string? search, Order<PlayerOrderBy> order, HistoryParams historyParams)
    {
        IQueryable<Player> query = _dbContext.Players.AsNoTracking().AsQueryable();
        query = ApplyQuerySearch(query, search);
        query = ApplyQueryOrder(query, order);

        int total = await query.CountAsync();
        IQueryable<Player> data = query.Skip((page - 1) * limit).Take(limit);
        Player[] players = await data.ToArrayAsync();
        Dictionary<int, PortfolioSnapshot[]> history = await _portfolioRepository.GetHistoryFor(
            players,
            historyParams.Step,
            historyParams.After
        );

        return new IndexResult<Player>([.. players.Select(p =>
        {
            if (history.TryGetValue(p.Id, out PortfolioSnapshot[]? snap))
            {
                p.History = [.. snap];
            }

            return p;
        })], total);
    }

    private static IQueryable<Player> ApplyQueryOrder(IQueryable<Player> query, Order<PlayerOrderBy> order)
    {
        return order.By switch
        {
            PlayerOrderBy.Name => query.OrderBy(p => p.Name),
            PlayerOrderBy.Credits => (order.Dir == OrderDirection.Ascending
                ? query.OrderBy(p => p.Credits)
                : query.OrderByDescending(p => p.Credits)).ThenBy(p => p.Name),
            PlayerOrderBy.Portfolio => (order.Dir == OrderDirection.Ascending
                ? query.OrderBy(p => p.Portfolio)
                : query.OrderByDescending(p => p.Portfolio)).ThenBy(p => p.Name),
            _ => throw new NotImplementedException(),
        };
    }

    private static IQueryable<Player> ApplyQuerySearch(IQueryable<Player> query, string? search)
    {
        if (search == null)
        {
            return query;
        }

        return query.Where(p => EF.Functions.ILike(p.Name, $"%{search}%"));
    }

    public async Task<Player?> GetPlayerData(Slug slug, HistoryParams historyParams)
    {
        Player? player = await _dbContext.Players
            .Include(u => u.Transactions.OrderBy(t => t.CreatedAt))
            .ThenInclude(t => t.Creator)
            .Include(u => u.LootBoxes.Where(l => l.ResultId == null))
            .ThenInclude(l => l.Result)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Slug == slug);

        if (player is null)
        {
            return null;
        }

        Dictionary<int, PortfolioSnapshot[]> history =
            await _portfolioRepository.GetHistoryFor([player], historyParams.Step, historyParams.After);

        if (history.TryGetValue(player.Id, out PortfolioSnapshot[]? portfolio))
        {
            player.History = [.. portfolio];
        }

        return player;
    }

    public void Add(Player player)
    {
        _dbContext.Add(player);
    }

    public Task<Player?> FindByPlatform(Platform platform, PlatformId platformId)
    {
        return _dbContext.Players.FirstOrDefaultAsync(p => p.Platform == platform && p.PlatformId == platformId);
    }

    public Task SaveChanges()
    {
        return _dbContext.SaveChangesAsync();
    }

    public ConfiguredCancelableAsyncEnumerable<Player> SeekAll(CancellationToken stoppingToken = default)
    {
        return _dbContext.Players
            .Include(p => p.Transactions.OrderBy(t => t.CreatedAt))
            .ThenInclude(t => t.Creator)
            .ToAsyncEnumerable()
            .WithCancellation(stoppingToken);
    }

    public void Update(Player player)
    {
        _dbContext.Update(player);
    }
}
