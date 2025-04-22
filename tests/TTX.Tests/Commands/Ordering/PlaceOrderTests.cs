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
        int credits = 25;
        int quantity = 1;
        int creatorValue = credits - 5;
        Creator creator = CreatorFactory.Create(value: creatorValue);
        Player player = PlayerFactory.Create(credits: credits);
        DbContext.Players.Add(player);
        DbContext.Creators.Add(creator);
        await DbContext.SaveChangesAsync();

        Transaction tx = await Sender.Send(new PlaceOrderCommand
        {
            Action = TransactionAction.Buy,
            Actor = player.Slug,
            Creator = creator.Slug,
            Amount = quantity,
        });

        Assert.AreEqual(quantity, tx.Quantity.Value);
        Assert.AreEqual(creatorValue, tx.Value);
        Assert.AreEqual(tx.Player, tx.Player);
        Assert.AreEqual(creator.Id, tx.Creator.Id);
        Assert.AreEqual(credits - creatorValue, tx.Player.Credits.Value);
        Assert.AreEqual(TransactionAction.Buy, tx.Action);
    }

    [TestMethod]
    public async Task PlaceSell_ShouldPass()
    {
        int credits = 25;
        int quantity = 1;
        int creatorValue = credits - 5;
        Player player = PlayerFactory.Create(credits: credits);
        Creator creator = CreatorFactory.Create(value: creatorValue);
        player.Buy(creator, quantity);
        DbContext.Players.Add(player);
        DbContext.Creators.Add(creator);
        await DbContext.SaveChangesAsync();

        Transaction tx = await Sender.Send(new PlaceOrderCommand
        {
            Action = TransactionAction.Sell,
            Actor = player.Slug,
            Creator = creator.Slug,
            Amount = quantity,
        });

        Assert.AreEqual(quantity, tx.Quantity.Value);
        Assert.AreEqual(creatorValue, tx.Value);
        Assert.AreEqual(tx.Player, tx.Player);
        Assert.AreEqual(creator.Id, tx.Creator.Id);
        Assert.AreEqual(credits, tx.Player.Credits.Value);
        Assert.AreEqual(TransactionAction.Sell, tx.Action);
    }
}
