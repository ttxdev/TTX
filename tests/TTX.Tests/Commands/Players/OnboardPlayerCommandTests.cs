using Microsoft.Extensions.DependencyInjection;
using TTX.Commands.Players.OnboardTwitchUser;
using TTX.Interfaces.Twitch;
using TTX.Tests.Factories;
using TTX.Tests.Infrastructure.Twitch;

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
}