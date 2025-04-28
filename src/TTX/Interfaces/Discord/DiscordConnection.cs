namespace TTX.Interfaces.Discord
{
    public readonly struct DiscordConnection
    {
        public string Id { get; init; }
        public string Type { get; init; }
        public bool Verified { get; init; }
    }
}