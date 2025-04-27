using TTX.Exceptions;
using TTX.Interfaces.Discord;
using TTX.Interfaces.Twitch;
using TTX.ValueObjects;

namespace TTX.Commands.Players.AuthenticateDiscordUser;

public class AuthenticateDiscordUserHandler(IDiscordAuthService discord, ITwitchAuthService twitch) : ICommandHandler<AuthenticateDiscordUserCommand, AuthenticateDiscordUserResult>
{
    public async Task<AuthenticateDiscordUserResult> Handle(AuthenticateDiscordUserCommand request, CancellationToken ct = default)
    {
        DiscordUser user = await discord.GetByOAuth(request.OAuthCode)
            ?? throw new DiscordUserNotFoundException();

        TwitchUser[] tUsers = await twitch.FindByIds([
            ..user.Connections
                .Where(c => c is { Verified: true, Type: "twitch" })
                .Select(c => (TwitchId)c.Id)
        ]);

        return new AuthenticateDiscordUserResult
        {
            User = user,
            TwitchUsers = tUsers
        };
    }
}
