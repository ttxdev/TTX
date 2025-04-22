using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Interfaces.Twitch;
using TTX.Models;

namespace TTX.Commands.Players.AuthenticateTwitchUser;

public class AuthenticateTwitchUserHandler(ApplicationDbContext context, ITwitchAuthService twitch) : ICommandHandler<AuthenticateTwitchUserCommand, Player>
{
    public async Task<Player> Handle(AuthenticateTwitchUserCommand request, CancellationToken ct)
    {
        var tUser = await twitch.FindByOAuth(request.OAuthCode) ?? throw new TwitchUserNotFoundException();

        Player? player = await context.Players.SingleOrDefaultAsync(p => p.TwitchId == tUser.Id);
        if (player is not null)
            return await Sync(tUser, player);

        player = Player.Create(
            name: tUser.DisplayName,
            slug: tUser.Login,
            twitchId: tUser.Id,
            avatarUrl: tUser.AvatarUrl
        );
        context.Players.Add(player);
        await context.SaveChangesAsync(ct);

        return player;
    }

    private async Task<Player> Sync(TwitchUser tUser, Player player)
    {
        if (player.Sync(
            name: tUser.DisplayName,
            slug: tUser.Login,
            avatarUrl: tUser.AvatarUrl
        ))
        {
            context.Players.Update(player);
            await context.SaveChangesAsync();
        }

        return player;
    }
}
