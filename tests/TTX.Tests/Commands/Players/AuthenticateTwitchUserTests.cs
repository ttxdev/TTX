using Microsoft.Extensions.DependencyInjection;
using TTX.Commands.Players.AuthenticateTwitchUser;
using TTX.Interfaces.Twitch;
using TTX.Notifications.Players;
using TTX.Tests.Factories;
using TTX.Tests.Infrastructure.Twitch;
using TTX.Tests.Notifications;

namespace TTX.Tests.Commands.Players;

[TestClass]
public class AuthenticateTwitchUserTests : ApplicationTests
{
    [TestMethod]
    public async Task AuthenticatePlayer_ShouldReturnPlayer()
    {
        var oauthCode = "1234567890";
        var player = PlayerFactory.Create();
        var tAuth = (TwitchAuthService)ServiceProvider.GetRequiredService<ITwitchAuthService>();
        tAuth.Inject(player, oauthCode);
        DbContext.Players.Add(player);
        await DbContext.SaveChangesAsync();

        var result = await Sender.Send(new AuthenticateTwitchUserCommand
        {
            OAuthCode = oauthCode
        });

        Assert.AreEqual(player.Slug, result.Slug);
        Assert.AreEqual(player.Name, result.Name);
        Assert.AreEqual(player.AvatarUrl, result.AvatarUrl);
        Assert.AreEqual(player.TwitchId, result.TwitchId);
    }

    [TestMethod]
    public async Task AuthenticateNewPlayer_ShouldReturnPlayer()
    {
        var oauthCode = "1234567890";
        var player = PlayerFactory.Create();
        var tAuth = (TwitchAuthService)ServiceProvider.GetRequiredService<ITwitchAuthService>();
        tAuth.Inject(player, oauthCode);

        var result = await Sender.Send(new AuthenticateTwitchUserCommand
        {
            OAuthCode = oauthCode
        });

        Assert.AreEqual(player.Slug, result.Slug);
        Assert.AreEqual(player.Name, result.Name);
        Assert.AreEqual(player.AvatarUrl, result.AvatarUrl);
        Assert.AreEqual(player.TwitchId, result.TwitchId);
    }

    [TestMethod]
    public async Task AuthenticateNewPlayer_ShouldNotifyCreatePlayer()
    {
        var oauthCode = "1234567890";
        var player = PlayerFactory.Create();
        var tAuth = (TwitchAuthService)ServiceProvider.GetRequiredService<ITwitchAuthService>();
        var cHandler = ServiceProvider.GetRequiredService<CreatePlayerNotificationHandler>();
        tAuth.Inject(player, oauthCode);

        await Sender.Send(new AuthenticateTwitchUserCommand
        {
            OAuthCode = oauthCode
        });

        var result = cHandler.FindNotification<CreatePlayer>(c => c.Slug == player.Slug);
        Assert.IsNotNull(result);
        Assert.AreEqual(player.Slug.Value, result.Slug);
        Assert.AreEqual(player.Name.Value, result.Name);
        Assert.AreEqual(player.AvatarUrl.ToString(), result.AvatarUrl);
        Assert.AreEqual(player.TwitchId.Value, result.TwitchId.Value);
    }
}