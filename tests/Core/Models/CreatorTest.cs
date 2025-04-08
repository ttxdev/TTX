using System.ComponentModel.DataAnnotations;
using TTX.Core.Models;
using TTX.Core.Exceptions;
using TTX.Tests.Core.Factories;

namespace TTX.Tests.Core.Models;

[TestClass]
public class CreatorTest
{
  [TestMethod]
  public void Create_ValidCreator_ShouldPassValidation()
  {
    var twitchUser = new TwitchUser
    {
      Id = "123",
      DisplayName = "TestUser",
      Login = "testuser",
      AvatarUrl = "https://example.com/avatar.jpg"
    };

    var creator = Creator.Create(twitchUser, "TEST");

    Assert.AreEqual("TestUser", creator.Name);
    Assert.AreEqual("testuser", creator.Slug);
    Assert.AreEqual("TEST", creator.Ticker);
    Assert.AreEqual("https://example.com/avatar.jpg", creator.AvatarUrl);
  }

  [TestMethod]
  [ExpectedException(typeof(ModelValidationException))]
  public void Create_InvalidSlug_ShouldThrowValidationException()
  {
    var twitchUser = new TwitchUser
    {
      Id = "123",
      DisplayName = "TestUser",
      Login = "invalid slug!",
      AvatarUrl = "https://example.com/avatar.jpg"
    };

    Creator.Create(twitchUser, "TEST");
  }

  [TestMethod]
  [ExpectedException(typeof(ModelValidationException))]
  public void Create_InvalidTicker_ShouldThrowValidationException()
  {
    var twitchUser = new TwitchUser
    {
      Id = "123",
      DisplayName = "TestUser",
      Login = "testuser",
      AvatarUrl = "https://example.com/avatar.jpg"
    };

    Creator.Create(twitchUser, "INVALID_TICKER");
  }

  [TestMethod]
  public void CreateVote_ShouldUpdateValueCorrectly()
  {
    var creator = new Creator
    {
      Name = "TestUser",
      Slug = "testuser",
      Ticker = "TEST",
      AvatarUrl = "https://example.com/avatar.jpg"
    };

    var vote = creator.CreateVote(10);
    Assert.AreEqual(10, creator.Value);
    Assert.AreEqual(10, vote.Value);

    vote = creator.CreateVote(-5);
    Assert.AreEqual(5, creator.Value);
    Assert.AreEqual(5, vote.Value);
  }

  [TestMethod]
  public void GetShares_ShouldCalculateSharesCorrectly()
  {
    var creator = CreatorFactory.Create();
    var user1 = UserFactory.Create();
    var user2 = UserFactory.Create();
    var user3 = UserFactory.Create();
    creator.Transactions = [
      TransactionFactory.CreateBuy(user1, creator, 5),
      TransactionFactory.CreateBuy(user2, creator, 10),
      TransactionFactory.CreateSell(user1, creator, 2),
      TransactionFactory.CreateSell(user2, creator, 5),
      TransactionFactory.CreateBuy(user3, creator, 3),
      TransactionFactory.CreateSell(user3, creator, 3)
    ];

    var shares = creator.GetShares();
    Assert.AreEqual(2, shares.Length);
    Assert.AreEqual(3, shares.First().Quantity); // User 1
    Assert.AreEqual(5, shares[1].Quantity); // User 2
    // User 3 should not be included as they sold all shares
  }
}