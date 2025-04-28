using TTX.Models;
using TTX.Tests.Factories;

namespace TTX.Tests.Models;

[TestClass]
public class CreatorTests
{
    [TestMethod]
    public void GetShares_ShouldReturnCorrectShares()
    {
        var shares = 5;
        var creatorValue = Creator.StarterValue;
        var credits = shares * creatorValue * 2;

        var playerOne = PlayerFactory.Create(credits, true);
        var playerTwo = PlayerFactory.Create(credits, true);
        var creator = CreatorFactory.Create(creatorValue);
        playerOne.Buy(creator, shares);
        playerTwo.Buy(creator, shares);

        Assert.AreEqual(2, creator.GetShares().Length);
        var share = creator.GetShares().Where(s => s.Creator == creator).FirstOrDefault();
        Assert.IsNotNull(share);
        Assert.AreEqual(shares, share.Quantity.Value);
    }
}