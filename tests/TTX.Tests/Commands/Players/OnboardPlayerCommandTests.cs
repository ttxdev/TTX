using Microsoft.Extensions.DependencyInjection;
using TTX.Commands.Players.OnboardTwitchUser;
using TTX.Interfaces.Twitch;
using TTX.Notifications.Players;
using TTX.Tests.Factories;
using TTX.Tests.Infrastructure.Twitch;
using TTX.Tests.Notifications;

namespace TTX.Tests.Commands.Players;

[TestClass]
public class OnboardPlayerCommandTests : ApplicationTests
{
    [TestMethod]
    public async Task OnboardPlayer_ShouldReturnPlayer()
    {
        var player = PlayerFactory.Create();
        var tAuth = (TwitchAuthService)ServiceProvider.GetRequiredService<ITwitchAuthService>();
        tAuth.Inject(player);

        var result = await Sender.Send(new OnboardTwitchUserCommand
        {
            Id = player.TwitchId
        });

        Assert.AreEqual(player.Slug, result.Slug);
        Assert.AreEqual(player.Name, result.Name);
        Assert.AreEqual(player.AvatarUrl, result.AvatarUrl);
        Assert.AreEqual(player.TwitchId, result.TwitchId);
    }

    [TestMethod]
    public async Task OnboardPlayer_ShouldNotifyCreatePlayer()
    {
        var player = PlayerFactory.Create();
        var tAuth = (TwitchAuthService)ServiceProvider.GetRequiredService<ITwitchAuthService>();
        var cHandler = ServiceProvider.GetRequiredService<CreatePlayerNotificationHandler>();
        tAuth.Inject(player);

        await Sender.Send(new OnboardTwitchUserCommand
        {
            Id = player.TwitchId
        });

        var result = cHandler.FindNotification<CreatePlayer>(c => c.Slug == player.Slug);
        Assert.IsNotNull(result);
        Assert.AreEqual(player.Slug.Value, result.Slug);
        Assert.AreEqual(player.Name.Value, result.Name);
        Assert.AreEqual(player.AvatarUrl.ToString(), result.AvatarUrl);
        Assert.AreEqual(player.TwitchId.Value, result.TwitchId.Value);
    }
}