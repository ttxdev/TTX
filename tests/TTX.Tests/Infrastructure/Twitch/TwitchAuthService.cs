using TTX.Interfaces.Twitch;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Tests.Infrastructure.Twitch;

public class TwitchAuthService : ITwitchAuthService
{
    private readonly HashSet<TwitchUser> _users = [];
    private readonly Dictionary<string, TwitchUser> _usersByOAuth = [];

    public Task<TwitchUser?> Find(Slug username)
    {
        var result = _users.FirstOrDefault(x => x.Login == username);
        return Task.FromResult(result);
    }

    public Task<TwitchUser?> FindById(TwitchId id)
    {
        var result = _users.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(result);
    }

    public Task<TwitchUser[]> FindByIds(TwitchId[] id)
    {
        var result = _users.Where(x => id.Contains(x.Id)).ToArray();
        return Task.FromResult(result);
    }

    public Task<TwitchUser?> FindByOAuth(string code)
    {
        var result = _usersByOAuth.GetValueOrDefault(code);
        return Task.FromResult(result);
    }

    public void Inject(TwitchUser user, string? oauth = null)
    {
        if (oauth is not null) _usersByOAuth.Add(oauth, user);
    }

    public void Inject(User user, string? oauth = null)
    {
        TwitchUser tUser = new()
        {
            Id = user.TwitchId,
            Login = user.Slug,
            DisplayName = user.Name,
            AvatarUrl = user.AvatarUrl
        };

        if (oauth is not null) _usersByOAuth.Add(oauth, tUser);

        _users.Add(tUser);
    }
}