using TTX.Core.Models;

namespace TTX.Core.Interfaces;

public interface ITwitchAuthService
{
    Task<TwitchUser?> Find(string username);
    Task<TwitchUser?> FindById(string id);
    Task<TwitchUser?> GetByOAuth(string code);
}