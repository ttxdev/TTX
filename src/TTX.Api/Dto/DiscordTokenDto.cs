using System.Text.Json.Serialization;
using TTX.Commands.Players.AuthenticateDiscordUser;

namespace TTX.Api.Dto;

public class DiscordTokenDto(AuthenticateDiscordUserResult result, string linkToken) : TokenDto(result.User.Token)
{
    [JsonPropertyName("link_token")] public string LinkToken { get; } = linkToken;

    [JsonPropertyName("twitch_users")]
    public TwitchUserDto[] TwitchUsers { get; } =
        result.TwitchUsers.Select(user => TwitchUserDto.Create(user)).ToArray();
}