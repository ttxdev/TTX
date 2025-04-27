using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Interfaces.Twitch;
using TTX.Models;

namespace TTX.Commands.Players.OnboardTwitchUser;

public class OnboardTwitchUserHandler(ApplicationDbContext context, ITwitchAuthService twitch) : ICommandHandler<OnboardTwitchUserCommand, Player>
{
    public async Task<Player> Handle(OnboardTwitchUserCommand request, CancellationToken ct = default)
    {
        TwitchUser tUser = await twitch.FindById(request.Id)
            ?? throw new TwitchUserNotFoundException();

        Player player = Player.Create(
            name: tUser.DisplayName,
            slug: tUser.Login,
            twitchId: tUser.Id,
            avatarUrl: tUser.AvatarUrl
        );

        context.Players.Add(player);
        await context.SaveChangesAsync(ct);

        return player;
    }
}
