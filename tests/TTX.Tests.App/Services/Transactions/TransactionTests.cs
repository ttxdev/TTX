using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TTX.App.Data;
using TTX.App.Dto.LootBoxes;
using TTX.App.Services.Transactions;
using TTX.App.Services.Transactions.Exceptions;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;

namespace TTX.Tests.App.Services.Transactions;

[TestClass]
public class TransactionTests : ServiceTests
{
    public virtual TestContext TestContext { get; set; }

    [TestMethod]
    public async Task PlayerCanBuy()
    {
        const int wallet = 200;
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TransactionService txService = scope.ServiceProvider.GetRequiredService<TransactionService>();
        Player player = _playerFactory.Create(credits: wallet);
        Creator creator = _creatorFactory.Create(value: wallet);
        dbContext.Players.Add(player);
        dbContext.Creators.Add(creator);
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);

        Result<ModelId> result = await txService.PlaceOrder(
            action: TransactionAction.Buy,
            quantity: 1,
            actorId: player.Id,
            creatorSlug: creator.Slug
        );

        Assert.IsNotNull(result.Value);
        Transaction? tx = await dbContext.Transactions.FirstOrDefaultAsync(tx => tx.Id == result.Value, TestContext.CancellationToken);
        Assert.IsNotNull(tx);
        Assert.AreEqual(0, player.Credits);
    }

    [TestMethod]
    public async Task PlayerCanSell()
    {
        const int creatorValue = 200;
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TransactionService txService = scope.ServiceProvider.GetRequiredService<TransactionService>();
        Player player = _playerFactory.Create(credits: 0);
        Creator creator = _creatorFactory.Create(value: creatorValue);
        dbContext.Players.Add(player);
        dbContext.Creators.Add(creator);
        player.Give(creator);
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);

        Result<ModelId> result = await txService.PlaceOrder(
            action: TransactionAction.Sell,
            quantity: 1,
            actorId: player.Id,
            creatorSlug: creator.Slug
        );

        Assert.IsNotNull(result.Value);
        Transaction? tx = await dbContext.Transactions.FirstOrDefaultAsync(tx => tx.Id == result.Value, TestContext.CancellationToken);
        Assert.IsNotNull(tx);
        Assert.AreEqual(creatorValue, player.Credits);
    }

    [TestMethod]
    public async Task PlayerCanGamble()
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TransactionService txService = scope.ServiceProvider.GetRequiredService<TransactionService>();
        Player player = _playerFactory.Create();
        LootBox lootBox = player.AddLootBox();
        Creator creator = _creatorFactory.Create(value: TransactionService.LootBoxMinValue);
        db.Players.Add(player);
        db.Creators.Add(creator);
        await db.SaveChangesAsync(TestContext.CancellationToken);

        Result<LootBoxResultDto> result = await txService.OpenLootBox(player.Id, lootBox.Id);

        Assert.IsNotNull(result.Value);
    }

    [TestMethod]
    public async Task PlayerCantReuseLootBox()
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TransactionService txService = scope.ServiceProvider.GetRequiredService<TransactionService>();
        Player player = _playerFactory.Create();
        LootBox lootBox = player.AddLootBox();
        Creator creator = _creatorFactory.Create(value: TransactionService.LootBoxMinValue);
        db.Players.Add(player);
        db.Creators.Add(creator);
        await db.SaveChangesAsync(TestContext.CancellationToken);
        await txService.OpenLootBox(player.Id, lootBox.Id);

        Result<LootBoxResultDto> result = await txService.OpenLootBox(player.Id, lootBox.Id);

        Assert.IsInstanceOfType<LootBoxOpenedException>(result.Error);
    }
}
