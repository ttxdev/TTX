using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TTX.App.Data;
using TTX.App.Data.Repositories;
using TTX.App.Dto.Creators;
using TTX.App.Dto.Pagination;
using TTX.App.Dto.Portfolio;
using TTX.App.Events;
using TTX.App.Events.Creators;
using TTX.App.Exceptions;
using TTX.App.Interfaces.Platforms;
using TTX.App.Services.Creators.Exceptions;
using TTX.App.Services.Creators.Validation;
using TTX.Domain.Exceptions;
using TTX.Domain.Models;
using TTX.Domain.Platforms;
using TTX.Domain.ValueObjects;

namespace TTX.App.Services.Creators;

public class CreatorService(
    ApplicationDbContext _dbContext,
    PortfolioRepository _portfolioRepository,
    IServiceProvider _services,
    IEventDispatcher _events
)
{
    public async Task<PaginationDto<CreatorDto>> Index(IndexCreatorsRequest request)
    {
        IQueryable<Creator> query = _dbContext.Creators.AsNoTracking().AsQueryable();

        if (request.MinValue.HasValue)
        {
            query = query.Where(c => c.Value >= request.MinValue.Value);
        }

        if (request.Search is not null)
        {
            query = query.Where(c => EF.Functions.ILike(c.Name, $"%{request.Search}%"));
        }

        Order<CreatorOrderBy> order = request.Order ?? new Order<CreatorOrderBy>
        {
            By = CreatorOrderBy.Name,
            Dir = OrderDirection.Ascending
        };

        query = order.By switch
        {
            CreatorOrderBy.Name => order.Dir == OrderDirection.Ascending
                            ? query.OrderBy(c => c.Name)
                            : query.OrderByDescending(c => c.Name),
            CreatorOrderBy.Value => (order.Dir == OrderDirection.Ascending
                ? query.OrderBy(c => c.Value)
                : query.OrderByDescending(c => c.Value)).ThenBy(c => c.Name),
            CreatorOrderBy.IsLive =>
                (order.Dir == OrderDirection.Ascending
                    ? query.OrderBy(c => c.StreamStatus.IsLive)
                    : query.OrderByDescending(c => c.StreamStatus.IsLive)).ThenBy(c => c.Name),
            _ => throw new NotImplementedException(),
        };

        int limit = int.Min(request.Limit, 100);
        int total = await query.CountAsync();
        Creator[] creators =
            await query.Skip((request.Page - 1) * limit).Take(limit).ToArrayAsync();

        Dictionary<int, Vote[]> history = await _portfolioRepository.GetHistoryFor(creators, request.HistoryParams.Step, request.HistoryParams.Before);

        return new PaginationDto<CreatorDto>
        {
            Total = total,
            Data = [.. creators.Select(c => {
                if (history.TryGetValue(c.Id, out Vote[]? cHistory))
                {
                    c.History = cHistory;
                }

                return CreatorDto.Create(c);
            })]
        };
    }

    public async Task<CreatorDto?> Find(Slug slug, HistoryParams historyParams)
    {
        Creator? creator = await _dbContext.Creators
            .Include(c => c.Transactions.OrderByDescending(t => t.CreatedAt))
            .ThenInclude(t => t.Player)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == slug);

        if (creator is null)
        {
            return null;
        }

        Dictionary<int, Vote[]> history =
            await _portfolioRepository.GetHistoryFor([creator], historyParams.Step, historyParams.Before);
        if (history.TryGetValue(creator.Id, out Vote[]? value))
        {
            creator.History = [.. value];
        }

        return CreatorDto.Create(creator);
    }

    public async Task<bool> IsPlayer(Slug creatorSlug, ModelId playerId)
    {
        PlatformRes? cPlatformRes = await _dbContext.Creators
            .Where(c => c.Slug == creatorSlug)
            .Select(c => new PlatformRes(c.Platform, c.PlatformId))
            .FirstOrDefaultAsync();
        if (cPlatformRes is null)
        {
            return false;
        }

        PlatformRes? pPlatformRes = await _dbContext.Players
            .Where(p => p.Id == playerId)
            .Select(p => new PlatformRes(p.Platform, p.PlatformId))
            .FirstOrDefaultAsync();

        return cPlatformRes == pPlatformRes;
    }

    public async Task<Result<CreatorOptOutDto>> OptOut(Slug slug, string reason = "")
    {
        Creator? creator = await _dbContext.Creators.FirstOrDefaultAsync(c => c.Slug == slug);
        if (creator is null)
        {
            return Result<CreatorOptOutDto>.Err(new NotFoundException<Creator>());
        }

        CreatorOptOut opt = CreatorOptOut.Create(creator, reason);
        _dbContext.CreatorOptOuts.Add(opt);
        _dbContext.Creators.Remove(creator);
        await _dbContext.SaveChangesAsync();

        return Result<CreatorOptOutDto>.Ok(CreatorOptOutDto.Create(opt));
    }

    public async Task<Result<ModelId>> Onboard(OnboardRequest request)
    {
        OnboardValidator validator = new(_dbContext);
        ValidationResult result = await validator.ValidateAsync(request);
        if (result.IsValid)
        {
            return Result<ModelId>.Err(new InvalidRequestException(result.Errors));
        }

        PlatformUser? user = null;
        IPlatformUserService platformUserService = _services.GetRequiredKeyedService<IPlatformUserService>(request.Platform);
        if (request.Username is not null)
        {
            user = await platformUserService.GetUserByUsername(request.Username);
        }
        else if (request.PlatformId is not null)
        {
            user = await platformUserService.GetUserById(request.PlatformId);
        }

        if (user is null)
        {
            return Result<ModelId>.Err(new NotFoundException<PlatformUser>());
        }

        Creator? creator = await _dbContext.Creators.FirstOrDefaultAsync(c => c.Platform == request.Platform && c.PlatformId == request.PlatformId);
        if (creator is not null)
        {
            if (creator.Sync(user))
            {
                await _dbContext.SaveChangesAsync();
            }

            return Result<ModelId>.Ok(creator.Id);
        }

        if (await _dbContext.CreatorOptOuts.AnyAsync(opt => opt.Platform == request.Platform && opt.PlatformId == user.Id))
        {
            return Result<ModelId>.Err(new CreatorOptedOutException());
        }

        creator = Creator.Create(user, request.Ticker, request.Platform);
        _dbContext.Creators.Add(creator);
        await _dbContext.SaveChangesAsync();
        await _events.Dispatch(CreateCreatorEvent.Create(creator));

        return Result<ModelId>.Ok(creator.Id);
    }

    private sealed record PlatformRes(Platform Platform, PlatformId PlatformId);
}
