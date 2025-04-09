using TTX.Core.Models;

namespace TTX.Core.Interfaces;

public interface IDiscordService
{
    Task<DiscordUser?> GetByOAuth(string code, ITwitchService twitchService);
    Task<TwitchUser?> GetByOAuthToTwitch(string code, string twitchId, ITwitchService twitchService);
}