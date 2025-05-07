using System.Text.Json.Serialization;

namespace TTX.Commands.Players.AuthenticateDiscordUser
{
    public readonly struct AuthenticateDiscordUserCommand : ICommand<AuthenticateDiscordUserResult>
    {
        [JsonPropertyName("code")] public required string OAuthCode { get; init; }
    }
}