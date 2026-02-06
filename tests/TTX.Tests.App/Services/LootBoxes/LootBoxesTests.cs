using Microsoft.Extensions.DependencyInjection;
using TTX.App.Data;
using TTX.App.Dto.LootBoxes;
using TTX.App.Services.LootBoxes;
using TTX.App.Services.LootBoxes.Exceptions;
using TTX.App.Services.Transactions;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;

namespace TTX.Tests.App.Services.LootBoxes;

[TestClass]
public class LootBoxesTests : ServiceTests
{
    [TestMethod]
    public async Task PlayerCanGamble()
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        LootBoxService lbService = scope.ServiceProvider.GetRequiredService<LootBoxService>();
        Random random = scope.ServiceProvider.GetRequiredService<Random>();
        Player player = _playerFactory.Create();
        LootBox lootBox = player.AddLootBox();
        Creator creator = _creatorFactory.Create(value: random.Next(1, 500));
        db.Players.Add(player);
        db.Creators.Add(creator);
        await db.SaveChangesAsync(TestContext.CancellationToken);

        Result<LootBoxResultDto> result = await lbService.OpenLootBox(player.Id, lootBox.Id);

        Assert.IsNotNull(result.Value);
    }

    [TestMethod]
    public async Task PlayerCantReuseLootBox()
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        LootBoxService lbService = scope.ServiceProvider.GetRequiredService<LootBoxService>();
        Random random = scope.ServiceProvider.GetRequiredService<Random>();
        Player player = _playerFactory.Create();
        LootBox lootBox = player.AddLootBox();
        Creator creator = _creatorFactory.Create(value: random.Next(1, 500));
        dbContext.Players.Add(player);
        dbContext.Creators.Add(creator);
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);
        await lbService.OpenLootBox(player.Id, lootBox.Id);

        Result<LootBoxResultDto> result = await lbService.OpenLootBox(player.Id, lootBox.Id);

        Assert.IsInstanceOfType<LootBoxOpenedException>(result.Error);
    }
}
