using Microsoft.Extensions.DependencyInjection;
using TTX.Commands.LootBoxes.OpenLootBox;
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
            DbContext.Creators.Add(CreatorFactory.Create(value: OpenLootBoxHandler.MinValue));
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
            DbContext.Creators.Add(CreatorFactory.Create(value: OpenLootBoxHandler.MinValue));
        DbContext.Players.Add(player);
        await DbContext.SaveChangesAsync();
        var lHandler = ServiceProvider.GetRequiredService<OpenLootBoxNotificationHandler>();

        await Sender.Send(new OpenLootBoxCommand
        {
            ActorId = player.Id,
            LootBoxId = lb.Id
        });

        var result = lHandler.FindNotification<TTX.Notifications.LootBoxes.OpenLootBox>(r => r.LootBoxId == lb.Id);
        Assert.IsNotNull(result);
    }
}