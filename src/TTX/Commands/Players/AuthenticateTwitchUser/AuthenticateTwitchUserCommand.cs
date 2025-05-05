using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Commands.Players.AuthenticateTwitchUser
{
    public class AuthenticateTwitchUserCommand : ICommand<Player>
    {
        [JsonPropertyName("code")] public string? OAuthCode { get; init; } = null;

        [JsonPropertyName("user_id")] public string? UserId { get; init; } = null;
    }
}