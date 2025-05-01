using TTX.Commands.Ordering.PlaceOrder;
using TTX.Models;
using TTX.Tests.Factories;

namespace TTX.Tests.Commands.Ordering;

[TestClass]
public class PlaceOrderTests : ApplicationTests
{
    [TestMethod]
    public async Task PlaceBuy_ShouldPass()
    {
        var credits = 50;
        var quantity = 1;
        var creatorValue = credits / 2;
        var creator = CreatorFactory.Create(creatorValue);
        var player = PlayerFactory.Create(credits);
        player.Buy(creator, quantity);
        DbContext.Players.Add(player);
        DbContext.Creators.Add(creator);
        await DbContext.SaveChangesAsync();

        var tx = await Sender.Send(new PlaceOrderCommand
        {
            Action = TransactionAction.Buy,
            Actor = player.Id,
            Creator = creator.Slug,
            Amount = quantity
        });

        Assert.AreEqual(quantity, tx.Quantity.Value);
        Assert.AreEqual(creatorValue, tx.Value);
        Assert.AreEqual(tx.Player, tx.Player);
        Assert.AreEqual(creator.Id, tx.Creator.Id);
        Assert.AreEqual(credits - (creatorValue * 2), tx.Player.Credits.Value);
        Assert.AreEqual(TransactionAction.Buy, tx.Action);
    }

    [TestMethod]
    public async Task PlaceSell_ShouldPass()
    {
        var credits = 25;
        var quantity = 1;
        var creatorValue = credits - 5;
        var player = PlayerFactory.Create(credits);
        var creator = CreatorFactory.Create(creatorValue);
        player.Buy(creator, quantity);
        DbContext.Players.Add(player);
        DbContext.Creators.Add(creator);
        await DbContext.SaveChangesAsync();

        var tx = await Sender.Send(new PlaceOrderCommand
        {
            Action = TransactionAction.Sell,
            Actor = player.Id,
            Creator = creator.Slug,
            Amount = quantity
        });

        Assert.AreEqual(quantity, tx.Quantity.Value);
        Assert.AreEqual(creatorValue, tx.Value);
        Assert.AreEqual(tx.Player, tx.Player);
        Assert.AreEqual(creator.Id, tx.Creator.Id);
        Assert.AreEqual(credits, tx.Player.Credits.Value);
        Assert.AreEqual(TransactionAction.Sell, tx.Action);
    }
}
