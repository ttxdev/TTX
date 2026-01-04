using System.Data;
using Microsoft.Extensions.DependencyInjection;
using TTX.App.Dto.Pagination;
using TTX.App.Dto.Players;
using TTX.App.Dto.Portfolio;
using TTX.App.Events;
using TTX.App.Events.Players;
using TTX.App.Interfaces.Platforms;
using TTX.App.Repositories;
using TTX.Domain.Exceptions;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;

namespace TTX.App.Services.Players;

public class PlayerService(
    IPlayerRepository _repository,
    IServiceProvider _services,
    IEventDispatcher _events
)
{
    public async Task<PaginationDto<PlayerDto>> Index(IndexPlayersRequest request)
    {
        IndexResult<Player> result = await _repository.Index(
            page: request.Page,
            search: request.Search,
            order: request.Order ?? new Order<PlayerOrderBy>()
            {
                By = PlayerOrderBy.Name,
                Dir = OrderDirection.Ascending
            },
            limit: request.Limit,
            history: request.HistoryParams
        );

        return new PaginationDto<PlayerDto>()
        {
            Data = [.. result.Data.Select(PlayerDto.Create)],
            Total = result.Total
        };
    }

    public async Task<PlayerDto?> Find(Slug slug, HistoryParams historyParams)
    {
        Player? player = await _repository.GetPlayerData(slug, historyParams);
        if (player is null)
        {
            return null;
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
        Player? player = await _repository.FindByPlatform(platform, pUser.Id);

        if (player is not null)
        {
            _repository.Update(player);
            if (player.Sync(name: pUser.DisplayName, slug: pUser.Username, avatarUrl: pUser.AvatarUrl))
            {
                await _repository.SaveChanges();
            }

            return Result<PlayerPartialDto>.Ok(PlayerPartialDto.Create(player));
        }

        player = Player.Create(name: pUser.DisplayName, slug: pUser.Username, platformId: pUser.Id, avatarUrl: pUser.AvatarUrl);
        _repository.Add(player);
        await _repository.SaveChanges();
        await _events.Dispatch(CreatePlayerEvent.Create(player));

        return Result<PlayerPartialDto>.Ok(PlayerPartialDto.Create(player));
    }
}
