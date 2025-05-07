using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TTX.Tests.Factories;
using TTX.Commands.Creators.CreatorOptOuts;
using TTX.Commands.Creators.OnboardTwitchCreator;
using TTX.Exceptions;
using TTX.Interfaces.Twitch;
using TTX.Tests.Infrastructure.Twitch;

namespace TTX.Tests.Commands.Creators;

[TestClass]
public class CreatorOptOutTests : ApplicationTests
{
    [TestMethod]
    public async Task OptOutCreator_ShouldPass()
    {
        var creator = CreatorFactory.Create();
        DbContext.Creators.Add(creator);
        await DbContext.SaveChangesAsync();

        var result = await Sender.Send(new CreatorOptOutCommand
        {
           Username = creator.Slug
        });

        Assert.IsNotNull(result);
        Assert.AreEqual(creator.TwitchId.Value, result.TwitchId);
        Assert.IsFalse(await DbContext.Creators.AnyAsync(c=> c.TwitchId == creator.TwitchId));
    }

    [TestMethod]
    public async Task OnboardOptOutCreator_ShouldFail()
    {
        var creator = CreatorFactory.Create();
        var tAuth = (TwitchAuthService)ServiceProvider.GetRequiredService<ITwitchAuthService>();
        tAuth.Inject(creator);
        DbContext.Creators.Add(creator);
        await DbContext.SaveChangesAsync();
        await Sender.Send(new CreatorOptOutCommand
        {
           Username = creator.Slug
        });

        await Assert.ThrowsExceptionAsync<InvalidActionException>(() => Sender.Send(new OnboardTwitchCreatorCommand
        {
            Ticker = creator.Ticker,
            Username = creator.Slug
        }));
    }
}
