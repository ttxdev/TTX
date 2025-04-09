namespace TTX.Core.Models;

public class DiscordUser {
    public required string access_token { get; set; }
    public required List<TwitchUser> TwitchUsers { get; set; }
}