using System.Collections.Immutable;

namespace TTX.Interfaces.Discord;

public readonly struct DiscordUser(DiscordTokenResponse token, ImmutableArray<DiscordConnection> connections)
{
    public DiscordTokenResponse Token { get; } = token;
    public ImmutableArray<DiscordConnection> TwitchConnections { get; } = [.. connections.Where(c => c.Verified && c.Type == "twitch")];
}