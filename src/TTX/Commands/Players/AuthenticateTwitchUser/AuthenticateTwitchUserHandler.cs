using MediatR;
using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Interfaces.Twitch;
using TTX.Models;
using TTX.Notifications.Players;

namespace TTX.Commands.Players.AuthenticateTwitchUser
{
    public class AuthenticateTwitchUserHandler(
        ApplicationDbContext context,
        IMediator mediator,
        ITwitchAuthService twitch)
        : ICommandHandler<AuthenticateTwitchUserCommand, Player>
    {
        public async Task<Player> Handle(AuthenticateTwitchUserCommand request, CancellationToken ct)
        {
            TwitchUser tUser = await FindTwitchUser(request);
            Player? player = await context.Players.SingleOrDefaultAsync(p => p.TwitchId == tUser.Id, ct);
            if (player is not null)
            {
                return await Sync(tUser, player);
            }

            player = Player.Create(
                tUser.DisplayName,
                tUser.Login,
                tUser.Id,
                tUser.AvatarUrl
            );
            context.Players.Add(player);
            await context.SaveChangesAsync(ct);
            await mediator.Publish(CreatePlayer.Create(player), ct);

            return player;
        }

        private async Task<TwitchUser> FindTwitchUser(AuthenticateTwitchUserCommand request)
        {
            TwitchUser? tUser;
            if (request.OAuthCode is not null)
            {
                tUser = await twitch.FindByOAuth(request.OAuthCode);
            }
            else if (request.UserId is not null)
            {
                tUser = await twitch.FindById(request.UserId);
            }
            else
            {
                throw new InvalidOperationException("Invalid request, Twitch identifier not provided");
            }

            return tUser ?? throw new NotFoundException<TwitchUser>();
        }

        private async Task<Player> Sync(TwitchUser tUser, Player player)
        {
            if (player.Sync(
                    tUser.DisplayName,
                    tUser.Login,
                    tUser.AvatarUrl
                ))
            {
                context.Players.Update(player);
                await context.SaveChangesAsync();
            }

            return player;
        }
    }
}