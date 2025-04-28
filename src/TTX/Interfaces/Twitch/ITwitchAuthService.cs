using TTX.ValueObjects;

namespace TTX.Interfaces.Twitch
{
    public interface ITwitchAuthService
    {
        Task<TwitchUser?> Find(Slug username);
        Task<TwitchUser?> FindById(TwitchId id);
        Task<TwitchUser[]> FindByIds(TwitchId[] id);
        Task<TwitchUser?> FindByOAuth(string code);
    }
}