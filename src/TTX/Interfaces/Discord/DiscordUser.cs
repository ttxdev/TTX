namespace TTX.Interfaces.Discord;

public readonly struct DiscordUser
{
    public required string Token { get; init; }
    public required DiscordConnection[] Connections { get; init; }
}