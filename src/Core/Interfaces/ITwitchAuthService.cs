using TTX.Core.Models;

namespace TTX.Core.Interfaces;

public interface ITwitchAuthService
{
    Task<TwitchUser?> Find(string username);
    Task<TwitchUser?> GetByOAuth(string code);
}