using TTX.Core.Models;

namespace TTX.Core.Interfaces;

public interface IDiscordAuthService
{
    Task<DiscordUser?> GetByOAuth(string code);
    Task<TwitchUser?> GetByOAuthToTwitch(string code, string twitchId);
}