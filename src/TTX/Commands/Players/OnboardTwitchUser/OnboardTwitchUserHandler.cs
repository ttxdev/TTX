using MediatR;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Interfaces.Twitch;
using TTX.Models;
using TTX.Notifications.Players;

namespace TTX.Commands.Players.OnboardTwitchUser
{
    public class OnboardTwitchUserHandler(ApplicationDbContext context, IMediator mediator, ITwitchAuthService twitch)
        : ICommandHandler<OnboardTwitchUserCommand, Player>
    {
        public async Task<Player> Handle(OnboardTwitchUserCommand request, CancellationToken ct = default)
        {
            TwitchUser tUser = await twitch.FindById(request.Id)
                               ?? throw new TwitchUserNotFoundException();

            Player player = Player.Create(
                tUser.DisplayName,
                tUser.Login,
                tUser.Id,
                tUser.AvatarUrl
            );

            context.Players.Add(player);
            await context.SaveChangesAsync(ct);
            await mediator.Publish(new CreatePlayer(player), ct);

            return player;
        }
    }
}