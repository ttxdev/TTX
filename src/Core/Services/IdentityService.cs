using TTX.Core.Exceptions;
using TTX.Core.Interfaces;
using TTX.Core.Models;

namespace TTX.Core.Services;

public interface IIdentityService
{
    Task<User> ProcessOAuth(string twitchUsername);
}


public class IdentityService(
  ITwitchAuthService twitch,
  IUserService userService
) : IIdentityService
{
    public async Task<User> ProcessOAuth(string oauthCode)
    {
        var tUser = await twitch.GetByOAuth(oauthCode) ?? throw new TwitchUserNotFoundException();
        var user = await userService.GetDetails(tUser.Login);
        if (user is not null)
            return user;

        return await userService.Onboard(tUser);
    }
}