using Microsoft.Extensions.Options;
using TTX.App.Interfaces.Platforms;
using TTX.Domain.ValueObjects;
using TTX.Infrastructure.Options;
using TwitchLib.Api;
using TwitchLib.Api.Auth;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace TTX.Infrastructure.Twitch;

public class TwitchUserService(IOptions<TwitchOAuthOptions> _options) : IPlatformUserService
{
    private readonly TwitchAPI _twitch = new()
    {
        Settings = {
            ClientId = _options.Value.ClientId,
            Secret = _options.Value.ClientSecret
        }
    };

    public async Task<PlatformUser?> GetUserById(PlatformId id)
    {
        GetUsersResponse resp = await _twitch.Helix.Users.GetUsersAsync([id]);
        User? user = resp.Users.FirstOrDefault();
        if (user is null)
        {
            return null;
        }

        return Convert(user);
    }

    public async Task<PlatformUser?> GetUserByUsername(Slug username)
    {
        GetUsersResponse resp = await _twitch.Helix.Users.GetUsersAsync(logins: [username]);
        User? user = resp.Users.FirstOrDefault();
        if (user is null)
        {
            return null;
        }

        return Convert(user);
    }

    public async Task<PlatformUser?> ResolveByOAuth(string code)
    {
        AuthCodeResponse? resp = await _twitch.Auth.GetAccessTokenFromCodeAsync(
            code: code,
            clientId: _options.Value.ClientId,
            clientSecret: _options.Value.ClientSecret,
            redirectUri: _options.Value.RedirectUri
        );
        string? token = resp.AccessToken;
        GetUsersResponse userResp = await _twitch.Helix.Users.GetUsersAsync(accessToken: token);
        User? user = userResp.Users.FirstOrDefault();
        if (user is null)
        {
            return null;
        }

        return Convert(user);
    }

    public Uri GetLoginUrl()
    {
        string redirectUri = _options.Value.RedirectUri;
        string clientId = _options.Value.ClientId;
        string scope = string.Join(' ', _options.Value.Scopes);

        return new Uri(
            $"https://id.twitch.tv/oauth2/authorize?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope={scope}"
        );
    }

    private static PlatformUser Convert(User user)
    {
        return new PlatformUser
        {
            Id = user.Id,
            DisplayName = Name.Create(user.DisplayName),
            Username = user.Login,
            AvatarUrl = new Uri(user.ProfileImageUrl)
        };
    }
}
