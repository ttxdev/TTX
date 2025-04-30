using Microsoft.Extensions.DependencyInjection;
using TTX.Commands.Creators.OnboardTwitchCreator;
using TTX.Interfaces.Twitch;
using TTX.Tests.Factories;
using TTX.Tests.Infrastructure.Twitch;

namespace TTX.Tests.Commands.Creators;

[TestClass]
public class OnboardCreatorCommandTests : ApplicationTests
{
    [TestMethod]
    public async Task OnboardCreator_ShouldReturnCreator()
    {
        var creator = CreatorFactory.Create();
        var tAuth = (TwitchAuthService)ServiceProvider.GetRequiredService<ITwitchAuthService>();
        tAuth.Inject(creator);

        var result = await Sender.Send(new OnboardTwitchCreatorCommand
        {
            Ticker = creator.Ticker,
            Username = creator.Slug
        });

        Assert.AreEqual(creator.Slug, result.Slug);
        Assert.AreEqual(creator.Name, result.Name);
        Assert.AreEqual(creator.AvatarUrl, result.AvatarUrl);
        Assert.AreEqual(creator.TwitchId, result.TwitchId);
        Assert.AreEqual(creator.Ticker, result.Ticker);
    }
}