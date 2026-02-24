using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TTX.App.Data;
using TTX.App.Dto.Players;
using TTX.App.Services.Players;
using TTX.Domain.Models;
using TTX.Domain.Platforms;
using TTX.Domain.ValueObjects;
using TTX.Tests.App.Factories;

namespace TTX.Tests.App.Services.Players;

[TestClass]
public class ManagePlayersTests : ServiceTests
{
    [TestMethod]
    public async Task PlayerStartsWithLootBox()
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        PlatformUserFactory platformUserFactory = scope.ServiceProvider.GetRequiredService<PlatformUserFactory>();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        PlatformUser user = platformUserFactory.Create();
        PlayerService playerService = scope.ServiceProvider.GetRequiredService<PlayerService>();
        Result<PlayerPartialDto> result = await playerService.Onboard(Platform.Twitch, user);
        if (!result.IsSuccessful)
        {
            Assert.Fail(result.Error!.ToString());
        }

        ModelId playerId = result.Value!.Id;
        Assert.IsTrue(await dbContext.LootBoxes.Where(l => l.PlayerId == playerId && l.ResultId == null).AnyAsync(TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task PlayerStartsWithAverageCreatorValue()
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        PlayerService playerService = scope.ServiceProvider.GetRequiredService<PlayerService>();
        PlatformUser user = _platformUserFactory.Create();
        Creator creator = _creatorFactory.Create(value: _random.Next(100, 1_000));
        dbContext.Creators.Add(creator);
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);

        Credits expected = await dbContext.Creators.AverageAsync(c => c.Value, TestContext.CancellationToken);
        Result<PlayerPartialDto> result = await playerService.Onboard(Platform.Twitch, user);
        if (!result.IsSuccessful)
        {
            Assert.Fail(result.Error!.ToString());
        }

        ModelId playerId = result.Value!.Id;
        Credits credits = await dbContext.Players.Where(p => p.Id == playerId).Select(p => p.Credits).FirstAsync(TestContext.CancellationToken);
        Assert.AreEqual(expected.Value, credits.Value);
    }
}
