using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using TTX.App.Dto.Pagination;
using TTX.App.Dto.Portfolio;
using TTX.App.Repositories;
using TTX.App.Services.Creators;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;

namespace TTX.Infrastructure.Data.Repositories;

public sealed class CreatorRepository(ApplicationDbContext _dbContext, PortfolioRepository _portfolioRepository) : ICreatorRepository
{
    public async Task<IndexResult<Creator>> Index(int page, int limit, int? minValue, string? search, Order<CreatorOrderBy> order, HistoryParams historyParams)
    {
        IQueryable<Creator> query = _dbContext.Creators.AsNoTracking().AsQueryable();
        query = ApplyQuerySearch(query, search);
        query = ApplyQueryOrder(query, order);
        if (minValue.HasValue)
        {
            query = query.Where(c => c.Value >= minValue.Value);
        }

        int total = await query.CountAsync();
        Creator[] creators =
            await query.Skip((page - 1) * limit).Take(limit).ToArrayAsync();

        Dictionary<int, Vote[]> history = await _portfolioRepository.GetHistoryFor(creators, historyParams.Step, historyParams.After);

        return new IndexResult<Creator>(creators, total);
    }

    public Task<bool> IsTickerTaken(Ticker ticker)
    {
        return _dbContext.Creators.AnyAsync(c => c.Ticker == ticker);
    }

    private static IQueryable<Creator> ApplyQuerySearch(IQueryable<Creator> query, string? search)
    {
        if (search is null)
        {
            return query;
        }

        return query.Where(c => EF.Functions.ILike(c.Name, $"%{search}%"));
    }

    private static IQueryable<Creator> ApplyQueryOrder(IQueryable<Creator> query, Order<CreatorOrderBy>? order)
    {
        Order<CreatorOrderBy> o = order ?? new Order<CreatorOrderBy>
        {
            By = CreatorOrderBy.Name,
            Dir = OrderDirection.Ascending
        };

        return o.By switch
        {
            CreatorOrderBy.Name => o.Dir == OrderDirection.Ascending
                            ? query.OrderBy(c => c.Name)
                            : query.OrderByDescending(c => c.Name),
            CreatorOrderBy.Value => (o.Dir == OrderDirection.Ascending
                ? query.OrderBy(c => c.Value)
                : query.OrderByDescending(c => c.Value)).ThenBy(c => c.Name),
            CreatorOrderBy.IsLive =>
                (o.Dir == OrderDirection.Ascending
                    ? query.OrderBy(c => c.StreamStatus.IsLive)
                    : query.OrderByDescending(c => c.StreamStatus.IsLive)).ThenBy(c => c.Name),
            _ => throw new NotImplementedException(),
        };
    }

    public async Task<Creator?> GetCreatorDetails(Slug slug, HistoryParams historyParams)
    {
        Creator? creator = await _dbContext.Creators
            .Include(c => c.Transactions.OrderBy(t => t.CreatedAt))
            .ThenInclude(t => t.Player)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == slug);

        if (creator is null)
        {
            return null;
        }

        Dictionary<int, Vote[]> history =
            await _portfolioRepository.GetHistoryFor([creator], historyParams.Step, historyParams.After);
        if (history.TryGetValue(creator.Id, out Vote[]? value))
        {
            creator.History = [.. value];
        }

        return creator;
    }

    public async Task<bool?> IsPlayer(Slug creatorSlug, ModelId playerId)
    {
        PlatformRes? cPlatformRes = await _dbContext.Creators
            .Where(c => c.Slug == creatorSlug)
            .Select(c => new PlatformRes(c.Platform, c.PlatformId))
            .FirstOrDefaultAsync();
        if (cPlatformRes is null)
        {
            return null;
        }

        PlatformRes? pPlatformRes = await _dbContext.Players
            .Where(p => p.Id == playerId)
            .Select(p => new PlatformRes(p.Platform, p.PlatformId))
            .FirstOrDefaultAsync();

        if (pPlatformRes is null)
        {
            return null;
        }

        return cPlatformRes == pPlatformRes;
    }

    public Task<Creator?> FindBySlug(Slug slug)
    {
        return _dbContext.Creators.FirstOrDefaultAsync(c => c.Slug == slug);
    }

    public void AddOptOut(CreatorOptOut opt)
    {
        _dbContext.CreatorOptOuts.Add(opt);
    }

    public void Remove(Creator creator)
    {
        _dbContext.Creators.Remove(creator);
    }

    public Task SaveChanges()
    {
        return _dbContext.SaveChangesAsync();
    }

    public Task<Creator?> FindByPlatform(Platform platform, PlatformId platformId)
    {
        return _dbContext.Creators.FirstOrDefaultAsync(c => c.Platform == platform && c.PlatformId == platformId);
    }

    public void Add(Creator creator)
    {
        _dbContext.Creators.Add(creator);
    }

    public Task<bool> IsOptOut(Platform platform, PlatformId platformId)
    {
        return _dbContext.CreatorOptOuts.AnyAsync(opt => opt.Platform == platform && opt.PlatformId == platformId);
    }

    public IAsyncEnumerable<Creator> GetAll()
    {
        return _dbContext.Creators.AsAsyncEnumerable();
    }

    public Task<Creator?> Find(ModelId creatorId)
    {
        return _dbContext.Creators.FirstOrDefaultAsync(c => c.Id == creatorId);
    }

    public Task StoreVote(Vote vote)
    {
        return _dbContext.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO votes (creator_id, value, time) VALUES ({vote.CreatorId.Value}, {vote.Value.Value}, {vote.Time})");
    }

    public void Update(Creator creator)
    {
        _dbContext.Creators.Update(creator);
    }

    private sealed record PlatformRes(Platform Platform, PlatformId PlatformId);
}
