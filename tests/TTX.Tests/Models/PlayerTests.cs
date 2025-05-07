using TTX.Exceptions;
using TTX.Models;
using TTX.Tests.Factories;

namespace TTX.Tests.Models;

[TestClass]
public class PlayerTests
{
    [TestMethod]
    public void GetShares_ShouldReturnCorrectShares()
    {
        var shares = 5;
        var creatorValue = Creator.StarterValue;
        var credits = shares * creatorValue * 2;

        var player = PlayerFactory.Create(credits: credits);
        var creatorOne = CreatorFactory.Create(value: creatorValue, includeId: true);
        var creatorTwo = CreatorFactory.Create(value: creatorValue, includeId: true);
        player.Buy(creatorOne, shares);
        player.Buy(creatorTwo, shares);

        Assert.AreEqual(2, player.GetShares().Length);
        var share = player.GetShares().Where(s => s.Creator == creatorOne).FirstOrDefault();
        Assert.IsNotNull(share);
        Assert.AreEqual(shares, share.Quantity.Value);
    }

    #region Ordering

    [TestMethod]
    public void ValidBuy_ShouldPass()
    {
        var credits = 25;
        var quantity = 1;
        var creatorValue = credits - 5;
        var creator = CreatorFactory.Create(creatorValue);
        var player = PlayerFactory.Create(credits);

        var tx = player.Buy(creator, quantity);

        Assert.AreEqual(quantity, tx.Quantity.Value);
        Assert.AreEqual(creatorValue, tx.Value);
        Assert.AreEqual(player, tx.Player);
        Assert.AreEqual(creator, tx.Creator);
        Assert.AreEqual(credits - creatorValue, player.Credits.Value);
        Assert.AreEqual(TransactionAction.Buy, tx.Action);
    }

    [TestMethod]
    public void ValidSell_ShouldPass()
    {
        var credits = 20;
        var quantity = 1;
        var creatorValue = credits + 5;
        var creator = CreatorFactory.Create(value: credits, includeId: true);
        var player = PlayerFactory.Create(credits: creatorValue);

        player.Buy(creator, quantity);
        var tx = player.Sell(creator, quantity);

        Assert.AreEqual(quantity, tx.Quantity.Value);
        Assert.AreEqual(credits, tx.Value);
        Assert.AreEqual(player, tx.Player);
        Assert.AreEqual(creator, tx.Creator);
        Assert.AreEqual(creatorValue, player.Credits);
        Assert.AreEqual(TransactionAction.Sell, tx.Action);
    }

    [TestMethod]
    public void BrokeBoy_ShouldCancelBuyTransaction()
    {
        var credits = 20;
        var creator = CreatorFactory.Create(credits + 5);
        var player = PlayerFactory.Create(credits);

        Assert.ThrowsException<InvalidActionException>(() => player.Buy(creator, 1));
        Assert.AreEqual(credits, player.Credits);
        Assert.AreEqual(0, player.Transactions.Count);
    }

    [TestMethod]
    public void NoShares_ShouldCancelSellTransaction()
    {
        var credits = 20;
        var creator = CreatorFactory.Create(value: credits + 5);
        var player = PlayerFactory.Create(credits: credits);

        Assert.ThrowsException<InvalidActionException>(() => player.Sell(creator, 1));
        Assert.AreEqual(credits, player.Credits);
        Assert.AreEqual(0, player.Transactions.Count);
    }

    [TestMethod]
    public void MaxShares_ShouldCancelSellTransaction()
    {
        var shares = Player.MaxShares + 1;
        var creatorValue = Creator.StarterValue;
        var credits = shares * creatorValue;
        var player = PlayerFactory.Create(credits: credits);
        var creator = CreatorFactory.Create(value: creatorValue, includeId: true);

        player.Buy(creator, Player.MaxShares);

        Assert.ThrowsException<InvalidActionException>(() => player.Buy(creator, 1));
        Assert.AreEqual(1, player.Credits);
        Assert.AreEqual(1, player.Transactions.Count);
    }

    #endregion
}
