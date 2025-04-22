using TTX.Models;
using TTX.Tests.Factories;

namespace TTX.Tests.Models;

[TestClass]
public class CreatorTests
{
    [TestMethod]
    public void GetShares_ShouldReturnCorrectShares()
    {
        int shares = 5;
        int creatorValue = Creator.StarterValue;
        int credits = shares * creatorValue * 2;

        Player playerOne = PlayerFactory.Create(credits: credits, includeId: true);
        Player playerTwo = PlayerFactory.Create(credits: credits, includeId: true);
        Creator creator = CreatorFactory.Create(value: creatorValue);
        playerOne.Buy(creator, shares);
        playerTwo.Buy(creator, shares);

        Assert.AreEqual(2, creator.GetShares().Length);
        Share? share = creator.GetShares().Where(s => s.Creator == creator).FirstOrDefault();
        Assert.IsNotNull(share);
        Assert.AreEqual(shares, share.Quantity.Value);
    }
}
