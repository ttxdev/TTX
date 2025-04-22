using TTX.Interfaces.Discord;
using TTX.Interfaces.Twitch;

namespace TTX.Commands.Players.AuthenticateDiscordUser;

public class AuthenticateDiscordUserResult
{
    public required DiscordUser User { get; init; }
    public required TwitchUser[] TwitchUsers { get; init; }
}
