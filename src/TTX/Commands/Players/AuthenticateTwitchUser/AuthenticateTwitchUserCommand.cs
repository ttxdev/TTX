using TTX.Models;

namespace TTX.Commands.Players.AuthenticateTwitchUser
{
    public class AuthenticateTwitchUserCommand : ICommand<Player>
    {
        public string? OAuthCode { get; init; } = null;
        public string? UserId { get; init; } = null;
    }
}