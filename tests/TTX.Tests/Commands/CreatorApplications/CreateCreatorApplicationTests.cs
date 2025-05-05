using Microsoft.Extensions.DependencyInjection;
using TTX.Commands.CreatorApplications.CreateCreatorApplication;
using TTX.Exceptions;
using TTX.Interfaces.Twitch;
using TTX.Tests.Factories;
using TTX.Tests.Infrastructure.Twitch;

namespace TTX.Tests.Commands.CreatorApplications;

[TestClass]
public class CreateCreatorApplicationTests : ApplicationTests
{
    [TestMethod]
    public async Task FreshApplication_ShouldPass()
    {
        var player = PlayerFactory.Create();
        DbContext.Players.Add(player);
        var creator = CreatorFactory.Create();
        var tAuth = (TwitchAuthService)ServiceProvider.GetRequiredService<ITwitchAuthService>();
        tAuth.Inject(creator);
        await DbContext.SaveChangesAsync();

        var result = await Sender.Send(new CreateCreatorApplicationCommand
        {
            SubmitterId = player.Id,
            Username = creator.Slug,
            Ticker = creator.Ticker
        });

        Assert.AreEqual(creator.TwitchId, result.TwitchId);
        Assert.AreEqual(creator.Ticker, result.Ticker);
    }

    [TestMethod]
    public async Task DupeTicker_ShouldFail()
    {
        var player = PlayerFactory.Create();
        DbContext.Players.Add(player);
        var creator = CreatorFactory.Create(ticker: "TEST");
        var conflict = CreatorFactory.Create(ticker: "TEST");
        DbContext.Creators.Add(conflict);
        await DbContext.SaveChangesAsync();

        await Assert.ThrowsExceptionAsync<InvalidActionException>(async () =>
        {
            await Sender.Send(new CreateCreatorApplicationCommand
            {
                SubmitterId = player.Id,
                Username = creator.Slug,
                Ticker = creator.Ticker
            });
        });
    }

    [TestMethod]
    public async Task DupeSlug_ShouldFail()
    {
        var player = PlayerFactory.Create();
        DbContext.Players.Add(player);
        var creator = CreatorFactory.Create(username: "test");
        var conflict = CreatorFactory.Create(username: "test");
        DbContext.Creators.Add(conflict);
        await DbContext.SaveChangesAsync();

        await Assert.ThrowsExceptionAsync<InvalidActionException>(async () =>
        {
            await Sender.Send(new CreateCreatorApplicationCommand
            {
                SubmitterId = player.Id,
                Username = creator.Slug,
                Ticker = creator.Ticker
            });
        });
    }
}
