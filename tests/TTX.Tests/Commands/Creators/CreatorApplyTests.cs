using Microsoft.Extensions.DependencyInjection;
using TTX.Commands.Creators.RecordNetChange;
using TTX.Tests.Infrastructure.Twitch;
using TTX.Interfaces.Twitch;
using TTX.Notifications.Creators;
using TTX.Tests.Factories;
using TTX.Tests.Notifications;
using TTX.Commands.Creators.CreatorApply;
using TTX.Exceptions;

namespace TTX.Tests.Commands.Creators;

[TestClass]
public class CreatorApplyTests : ApplicationTests
{
    [TestMethod]
    public async Task FreshApplication_ShouldPass()
    {
        var creator = CreatorFactory.Create();
        var tAuth = (TwitchAuthService)ServiceProvider.GetRequiredService<ITwitchAuthService>();
        tAuth.Inject(creator);

        var result = await Sender.Send(new CreatorApplyCommand
        {
            Username = creator.Slug,
            Ticker = creator.Ticker
        });

        Assert.AreEqual(creator.TwitchId, result.TwitchId);
        Assert.AreEqual(creator.Ticker, result.Ticker);
    }

    [TestMethod]
    public async Task DupeTicker_ShouldFail()
    {
        var creator = CreatorFactory.Create(ticker: "TEST");
        var conflict = CreatorFactory.Create(ticker: "TEST");
        DbContext.Creators.Add(conflict);
        await DbContext.SaveChangesAsync();

        Assert.ThrowsException<CreatorTickerTakenException>(async () =>
        {
            await Sender.Send(new CreatorApplyCommand
            {
                Username = creator.Slug,
                Ticker = creator.Ticker
            });
        });
    }

    [TestMethod]
    public async Task DupeSlug_ShouldFail()
    {
        var creator = CreatorFactory.Create(username: "test");
        var conflict = CreatorFactory.Create(username: "test");
        DbContext.Creators.Add(conflict);
        await DbContext.SaveChangesAsync();

        Assert.ThrowsException<CreatorExistsException>(async () =>
        {
            await Sender.Send(new CreatorApplyCommand
            {
                Username = creator.Slug,
                Ticker = creator.Ticker
            });
        });
    }
}
