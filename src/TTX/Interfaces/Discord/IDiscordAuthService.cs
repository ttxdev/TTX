namespace TTX.Interfaces.Discord
{
    public interface IDiscordAuthService
    {
        Task<DiscordUser?> GetByOAuth(string code);
    }
}