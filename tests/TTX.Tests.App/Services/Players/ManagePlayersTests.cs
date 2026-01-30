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
}
