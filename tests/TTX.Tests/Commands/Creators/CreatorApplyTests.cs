using Microsoft.Extensions.DependencyInjection;
using TTX.Commands.Creators.RecordNetChange;
using TTX.Tests.Infrastructure.Twitch;
using TTX.Interfaces.Twitch;
using TTX.Notifications.Creators;
using TTX.Tests.Factories;
using TTX.Tests.Notifications;
using TTX.Commands.Creators.CreatorApply;

namespace TTX.Tests.Commands.Creators;

[TestClass]
public class CreatorApplyTests : ApplicationTests
{
    [TestMethod]
    public async Task Applicant_ShouldPass()
    {
        var creator = CreatorFactory.Create();
        var tAuth = (TwitchAuthService)ServiceProvider.GetRequiredService<ITwitchAuthService>();
        tAuth.Inject(creator);

        var result = await Sender.Send(new CreatorApplyCommand
        {
            Username = creator.Slug,
            Ticker = creator.Ticker
        });

        Assert.AreEqual(creator.Slug, result.Slug);
        Assert.AreEqual(creator.Ticker, result.Ticker);
    }
}
