using TTX.Core.Interfaces;
using TTX.Core.Models;
using TwitchLib.Api;

namespace TTX.Interface.Api.Services;

public class TwitchService : ITwitchService
{
    private readonly TwitchAPI twitch;
    private readonly string clientId;
    private readonly string clientSecret;
    private readonly string redirectUri;
    public TwitchService(string clientId, string clientSecret, string redirectUri)
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


    public async Task<TwitchUser?> Find(string username)
    {
        var user = await twitch.Helix.Users.GetUsersAsync(logins: [username]).ContinueWith(
          t => t.Result.Users.Length == 0 ? null : t.Result.Users[0]
        );
        if (user is null)
            return null;

        return new TwitchUser
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            AvatarUrl = user.ProfileImageUrl,
            Login = user.Login,
        };
    }

    public async Task<TwitchUser?> GetByOAuth(string code)
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
            DisplayName = user.DisplayName,
            AvatarUrl = user.ProfileImageUrl,
            Login = user.Login,
        };
    }
}
