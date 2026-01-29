using System.Data;
using Microsoft.Extensions.DependencyInjection;
using TTX.App.Dto.Pagination;
using TTX.App.Dto.Players;
using TTX.App.Dto.Portfolio;
using TTX.App.Events;
using TTX.App.Events.Players;
using TTX.App.Interfaces.Platforms;
using TTX.Domain.Exceptions;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;
using TTX.Domain.Platforms;
using TTX.App.Data;
using TTX.App.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TTX.App.Services.Players;

public class PlayerService(
    ApplicationDbContext _dbContext,
    PortfolioRepository _portfolioRepository,
    IServiceProvider _services,
    IEventDispatcher _events
)
{
    public async Task<PaginationDto<PlayerDto>> Index(IndexPlayersRequest request)
    {
        IQueryable<Player> query = _dbContext.Players.AsNoTracking().AsQueryable();
        if (request.Search is not null)
        {
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{request.Search}%"));
        }

        Order<PlayerOrderBy> order = request.Order ?? new()
        {
            By = PlayerOrderBy.Name,
            Dir = OrderDirection.Ascending
        };
        query = order.By switch
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

        int limit = int.Max(request.Limit, 100);
        int total = await query.CountAsync();
        IQueryable<Player> data = query.Skip((request.Page - 1) * limit).Take(limit);
        Player[] players = await data.ToArrayAsync();
        Dictionary<int, PortfolioSnapshot[]> history = await _portfolioRepository.GetHistoryFor(
            players,
            request.HistoryParams.Step,
            request.HistoryParams.After
        );

        return new PaginationDto<PlayerDto>()
        {
            Total = total,
            Data = [.. players.Select(p =>
                {
                    if (history.TryGetValue(p.Id, out PortfolioSnapshot[]? snap))
                    {
                        p.History = [.. snap];
                    }


                    return PlayerDto.Create(p);
                })]
        };
    }

    public async Task<PlayerDto?> Find(Slug slug, HistoryParams historyParams)
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

        return PlayerDto.Create(player);
    }

    public async Task<Result<PlayerPartialDto>> Authenticate(Platform platform, string oauthCode)
    {
        IPlatformUserService platformUserService = _services.GetRequiredKeyedService<IPlatformUserService>(platform);
        PlatformUser? pUser = await platformUserService.ResolveByOAuth(oauthCode);
        if (pUser is null)
        {
            return Result<PlayerPartialDto>.Err(new NotFoundException<PlatformUser>());
        }

        return await Onboard(platform, pUser);
    }

    public async Task<Result<PlayerPartialDto>> Onboard(Platform platform, PlatformUser pUser)
    {
        Player? player = await _dbContext.Players.FirstOrDefaultAsync(p => p.Platform == platform && p.PlatformId == pUser.Id);

        if (player is not null)
        {
            if (player.Sync(pUser))
            {
                await _dbContext.SaveChangesAsync();
            }

            return Result<PlayerPartialDto>.Ok(PlayerPartialDto.Create(player));
        }

        player = Player.Create(pUser);
        _dbContext.Players.Add(player);
        await _dbContext.SaveChangesAsync();
        await _events.Dispatch(CreatePlayerEvent.Create(player));

        return Result<PlayerPartialDto>.Ok(PlayerPartialDto.Create(player));
    }
}
