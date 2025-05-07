using System.Text.Json.Serialization;
using TTX.Interfaces.Discord;
using TTX.Interfaces.Twitch;

namespace TTX.Commands.Players.AuthenticateDiscordUser
{
    public class AuthenticateDiscordUserResult
    {
        [JsonPropertyName("user")]
        public required DiscordUser User { get; init; }
        [JsonPropertyName("twitch_users")]
        public required TwitchUser[] TwitchUsers { get; init; }
    }
}