using TTX.Core.Exceptions;
using TTX.Core.Interfaces;
using TTX.Core.Models;

namespace TTX.Core.Services;

public interface IIdentityService
{
    Task<User> ProcessTwitchOAuth(string oauthCode);
    Task<DiscordUser> ProcessDiscordOAuth(string oauthCode);
    Task<User> ProcessDiscordToTwitchOAuth(string oauthCode, string twitchId);
}


public class IdentityService(
  ITwitchAuthService twitch,
  IDiscordAuthService discord,
  IUserService userService
) : IIdentityService
{
    public async Task<User> ProcessTwitchOAuth(string oauthCode)
    {
        var tUser = await twitch.GetByOAuth(oauthCode) ?? throw new TwitchUserNotFoundException();
        var user = await userService.GetDetails(tUser.Login);
        if (user is not null)
            return user;

        return await userService.Onboard(tUser);
    }

    public async Task<DiscordUser> ProcessDiscordOAuth(string oauthCode)
    {
        return await discord.GetByOAuth(oauthCode) ?? throw new DiscordUserNotFoundException();
    }

    public async Task<User> ProcessDiscordToTwitchOAuth(string oauthCode, string twitchId)
    {
        var tUser = await discord.GetByOAuthToTwitch(oauthCode, twitchId) ?? throw new TwitchUserNotFoundException();
        var user = await userService.GetDetails(tUser.DisplayName);
        if (user is not null)
            return user;

        return await userService.Onboard(tUser);
    }
}