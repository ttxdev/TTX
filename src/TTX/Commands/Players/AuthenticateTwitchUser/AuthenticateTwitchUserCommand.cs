using System.Text.Json.Serialization;
using TTX.Dto.Players;
using TTX.Models;

namespace TTX.Commands.Players.AuthenticateTwitchUser
{
    public class AuthenticateTwitchUserCommand : ICommand<PlayerDto>
    {
        [JsonPropertyName("code")] public string? OAuthCode { get; init; } = null;

        [JsonPropertyName("user_id")] public string? UserId { get; init; } = null;
    }
}