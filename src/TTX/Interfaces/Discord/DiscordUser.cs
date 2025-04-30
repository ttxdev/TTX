namespace TTX.Interfaces.Discord
{
    public class DiscordUser
    {
        public required string Token { get; init; }
        public required DiscordConnection[] Connections { get; init; }
    }
}