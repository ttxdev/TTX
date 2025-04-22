using TTX.Interfaces.Twitch;
using TTX.ValueObjects;
using TwitchLib.Api;

namespace TTX.Infrastructure.Twitch;

public class TwitchAuthService : ITwitchAuthService
{
    private readonly TwitchAPI twitch;
    private readonly string clientId;
    private readonly string clientSecret;
    private readonly string redirectUri;

    public TwitchAuthService(string clientId, string clientSecret, string redirectUri)
    {
        this.clientId = clientId;
        this.clientSecret = clientSecret;
        this.redirectUri = redirectUri;
        twitch = new TwitchAPI
        {
            Settings = {
                ClientId = clientId,
                Secret = clientSecret
            }
        };
    }

    public async Task<TwitchUser?> Find(Slug username)
    {
        var user = await twitch.Helix.Users.GetUsersAsync(logins: [username]).ContinueWith(
          t => t.Result.Users.Length == 0 ? null : t.Result.Users[0]
        );

        if (user is null)
            return null;

        return new TwitchUser
        {
            Id = user.Id,
            DisplayName = Name.Create(user.DisplayName),
            AvatarUrl = new Uri(user.ProfileImageUrl),
            Login = user.Login,
        };
    }

    public async Task<TwitchUser?> FindById(TwitchId id)
    {
        var user = await twitch.Helix.Users.GetUsersAsync(ids: [id]).ContinueWith(
          t => t.Result.Users.Length == 0 ? null : t.Result.Users[0]
        );
        if (user is null)
            return null;

        return new TwitchUser
        {
            Id = user.Id,
            DisplayName = Name.Create(user.DisplayName),
            AvatarUrl = new Uri(user.ProfileImageUrl),
            Login = user.Login,
        };
    }

    public Task<TwitchUser[]> FindByIds(TwitchId[] ids)
    {
        return twitch.Helix.Users.GetUsersAsync(ids: [.. ids.Select(id => id.Value)])
          .ContinueWith(t => t.Result.Users.Select(u => new TwitchUser
          {
              Id = u.Id,
              DisplayName = Name.Create(u.DisplayName),
              AvatarUrl = new Uri(u.ProfileImageUrl),
              Login = u.Login,
          }).ToArray());
    }

    public async Task<TwitchUser?> FindByOAuth(string code)
    {
        var resp = await twitch.Auth.GetAccessTokenFromCodeAsync(code: code, clientId: clientId, clientSecret: clientSecret, redirectUri: redirectUri);
        var token = resp.AccessToken;
        var user = await twitch.Helix.Users.GetUsersAsync(accessToken: token)
          .ContinueWith(t => t.Result.Users.Length == 0 ? null : t.Result.Users[0]);
        if (user is null)
            return null;

        return new TwitchUser
        {
            Id = user.Id,
            DisplayName = Name.Create(user.DisplayName),
            AvatarUrl = new Uri(user.ProfileImageUrl),
            Login = user.Login,
        };
    }
}
