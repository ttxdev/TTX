using TTX.Exceptions;
using TTX.Models;
using TTX.Tests.Factories;

namespace TTX.Tests.Models;

[TestClass]
public class PlayerTests
{
    #region Ordering
    [TestMethod]
    public void ValidBuy_ShouldPass()
    {
        int credits = 25;
        int quantity = 1;
        int creatorValue = credits - 5;
        Creator creator = CreatorFactory.Create(value: creatorValue);
        Player player = PlayerFactory.Create(credits: credits);

        Transaction tx = player.Buy(creator, quantity);

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
        int credits = 20;
        int quantity = 1;
        int creatorValue = credits + 5;
        Creator creator = CreatorFactory.Create(value: credits, includeId: true);
        Player player = PlayerFactory.Create(credits: creatorValue);

        player.Buy(creator, quantity);
        Transaction tx = player.Sell(creator, quantity);

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
        int credits = 20;
        Creator creator = CreatorFactory.Create(value: credits + 5);
        Player player = PlayerFactory.Create(credits: credits);

        Assert.ThrowsException<ExceedsBalanceException>(() => player.Buy(creator, 1));
        Assert.AreEqual(credits, player.Credits);
        Assert.AreEqual(0, player.Transactions.Count);
    }

    [TestMethod]
    public void NoShares_ShouldCancelSellTransaction()
    {
        int credits = 20;
        Creator creator = CreatorFactory.Create(value: credits + 5);
        Player player = PlayerFactory.Create(credits: credits);

        Assert.ThrowsException<ExceedsSharesException>(() => player.Sell(creator, 1));
        Assert.AreEqual(credits, player.Credits);
        Assert.AreEqual(0, player.Transactions.Count);
    }

    [TestMethod]
    public void MaxShares_ShouldCancelSellTransaction()
    {
        int shares = Player.MaxShares + 1;
        int creatorValue = Creator.StarterValue;
        int credits = shares * creatorValue;
        Player player = PlayerFactory.Create(credits: credits);
        Creator creator = CreatorFactory.Create(value: creatorValue, includeId: true);

        player.Buy(creator, Player.MaxShares);

        Assert.ThrowsException<MaxSharesException>(() => player.Buy(creator, 1));
        Assert.AreEqual(1, player.Credits);
        Assert.AreEqual(1, player.Transactions.Count);
    }
    #endregion

    [TestMethod]
    public void GetShares_ShouldReturnCorrectShares()
    {
        int shares = 5;
        int creatorValue = Creator.StarterValue;
        int credits = shares * creatorValue * 2;

        Player player = PlayerFactory.Create(credits: credits);
        Creator creatorOne = CreatorFactory.Create(value: creatorValue, includeId: true);
        Creator creatorTwo = CreatorFactory.Create(value: creatorValue, includeId: true);
        player.Buy(creatorOne, shares);
        player.Buy(creatorTwo, shares);

        Assert.AreEqual(2, player.GetShares().Length);
        Share? share = player.GetShares().Where(s => s.Creator == creatorOne).FirstOrDefault();
        Assert.IsNotNull(share);
        Assert.AreEqual(shares, share.Quantity.Value);
    }
}
