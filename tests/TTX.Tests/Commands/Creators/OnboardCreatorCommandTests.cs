using Microsoft.Extensions.DependencyInjection;
using TTX.Commands.Creators.OnboardTwitchCreator;
using TTX.Interfaces.Twitch;
using TTX.Notifications.Creators;
using TTX.Tests.Factories;
using TTX.Tests.Infrastructure.Twitch;
using TTX.Tests.Notifications;
using TTX.Exceptions;

namespace TTX.Tests.Commands.Creators;

[TestClass]
public class OnboardCreatorCommandTests : ApplicationTests
{
    [TestMethod]
    public async Task OnboardUsername_ShouldReturnCreator()
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

    [TestMethod]
    public async Task OnboardTwitchId_ShouldReturnCreator()
    {
        var creator = CreatorFactory.Create();
        var tAuth = (TwitchAuthService)ServiceProvider.GetRequiredService<ITwitchAuthService>();
        tAuth.Inject(creator);

        var result = await Sender.Send(new OnboardTwitchCreatorCommand
        {
            Ticker = creator.Ticker,
            TwitchId = creator.TwitchId
        });

        Assert.AreEqual(creator.Slug, result.Slug);
        Assert.AreEqual(creator.Name, result.Name);
        Assert.AreEqual(creator.AvatarUrl, result.AvatarUrl);
        Assert.AreEqual(creator.TwitchId, result.TwitchId);
        Assert.AreEqual(creator.Ticker, result.Ticker);
    }

    [TestMethod]
    public async Task OnboardCreator_ShouldNotifyCreateCreator()
    {
        var creator = CreatorFactory.Create();
        var tAuth = (TwitchAuthService)ServiceProvider.GetRequiredService<ITwitchAuthService>();
        var cHandler = ServiceProvider.GetRequiredService<CreateCreatorNotificationHandler>();
        tAuth.Inject(creator);

        await Sender.Send(new OnboardTwitchCreatorCommand
        {
            Ticker = creator.Ticker,
            Username = creator.Slug
        });

        var result = cHandler.FindNotification<CreateCreator>(c => c.Creator.Slug == creator.Slug);
        Assert.IsNotNull(result);
        Assert.AreEqual(creator.Slug.Value, result.Creator.Slug);
        Assert.AreEqual(creator.Name.Value, result.Creator.Name);
        Assert.AreEqual(creator.AvatarUrl.ToString(), result.Creator.AvatarUrl);
        Assert.AreEqual(creator.TwitchId.Value, result.Creator.TwitchId);
        Assert.AreEqual(creator.Ticker.Value, result.Creator.Ticker);
    }

    [TestMethod]
    public async Task DupeTicker_ShouldFail()
    {
        var creator = CreatorFactory.Create();
        var conflict = CreatorFactory.Create(ticker: creator.Ticker);
        DbContext.Creators.Add(conflict);
        await DbContext.SaveChangesAsync();
        var tAuth = (TwitchAuthService)ServiceProvider.GetRequiredService<ITwitchAuthService>();
        var cHandler = ServiceProvider.GetRequiredService<CreateCreatorNotificationHandler>();
        tAuth.Inject(creator);

        await Assert.ThrowsExceptionAsync<InvalidActionException>(() => Sender.Send(new OnboardTwitchCreatorCommand
        {
            Ticker = creator.Ticker,
            Username = creator.Slug
        }));
    }
}
