using Microsoft.Extensions.DependencyInjection;
using TTX.Commands.LootBoxes.OpenLootBox;
using TTX.Notifications.LootBoxes;
using TTX.Notifications.Transactions;
using TTX.Tests.Factories;
using TTX.Tests.Notifications;

namespace TTX.Tests.Commands.LootBoxes;

[TestClass]
public class OpenLootBoxTests : ApplicationTests
{
    [TestMethod]
    public async Task OpenLootBox_ShouldProvideResult()
    {
        var player = PlayerFactory.Create();
        var lb = player.AddLootBox();
        foreach (var _ in Enumerable.Range(0, 20))
            DbContext.Creators.Add(CreatorFactory.Create(OpenLootBoxHandler.MinValue));
        DbContext.Players.Add(player);
        await DbContext.SaveChangesAsync();

        var result = await Sender.Send(new OpenLootBoxCommand
        {
            ActorId = player.Id,
            LootBoxId = lb.Id
        });

        Assert.IsNotNull(result.Result.Creator);
    }

    [TestMethod]
    public async Task OpenLootBox_ShouldNotifyLootBoxResult()
    {
        var player = PlayerFactory.Create();
        var lb = player.AddLootBox();
        foreach (var _ in Enumerable.Range(0, 20))
            DbContext.Creators.Add(CreatorFactory.Create(OpenLootBoxHandler.MinValue));
        DbContext.Players.Add(player);
        await DbContext.SaveChangesAsync();
        var lHandler = ServiceProvider.GetRequiredService<OpenLootBoxNotificationHandler>();

        await Sender.Send(new OpenLootBoxCommand
        {
            ActorId = player.Id,
            LootBoxId = lb.Id
        });

        var result = lHandler.FindNotification<OpenLootBox>(r => r.LootBoxId == lb.Id);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task OpenLootBox_ShouldNotifyCreateTransaction()
    {
        var player = PlayerFactory.Create();
        var lb = player.AddLootBox();
        foreach (var _ in Enumerable.Range(0, 20))
            DbContext.Creators.Add(CreatorFactory.Create(OpenLootBoxHandler.MinValue));
        DbContext.Players.Add(player);
        await DbContext.SaveChangesAsync();
        var lHandler = ServiceProvider.GetRequiredService<CreateTransactionNotificationHandler>();

        await Sender.Send(new OpenLootBoxCommand
        {
            ActorId = player.Id,
            LootBoxId = lb.Id
        });

        var result = lHandler.FindNotification<CreateTransaction>(t => t.PlayerId == player.Id);
        Assert.IsNotNull(result);
    }
}
