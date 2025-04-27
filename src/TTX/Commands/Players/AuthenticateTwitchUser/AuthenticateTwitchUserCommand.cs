using TTX.Models;

namespace TTX.Commands.Players.AuthenticateTwitchUser;

public class AuthenticateTwitchUserCommand : ICommand<Player>
{
    public required string OAuthCode { get; init; }
}
