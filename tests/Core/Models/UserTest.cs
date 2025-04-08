// filepath: /home/dylhack/PARA/2/ttx/TTX/tests/TTX.Core/Models/UserTest.cs
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;
using TTX.Core.Models;
using TTX.Core.Exceptions;
using TTX.Tests.Core.Factories;

namespace TTX.Tests.Core.Models;

[TestClass]
public class UserTest
{
  [TestMethod]
  public void Validate_ValidUser_ShouldPassValidation()
  {
    var user = UserFactory.Create();
    var validationResults = new List<ValidationResult>();
    var isValid = Validator.TryValidateObject(user, new ValidationContext(user), validationResults, true);

    Assert.IsTrue(isValid);
    Assert.AreEqual(0, validationResults.Count);
  }

  [TestMethod]
  public void Buy_ValidTransaction_ShouldDeductCredits()
  {
    var user = UserFactory.Create();
    var creator = CreatorFactory.Create();
    var initialCredits = user.Credits;

    var transaction = user.Buy(creator, 10);

    Assert.AreEqual(initialCredits - (creator.Value * 10), user.Credits);
    Assert.AreEqual(TransactionAction.Buy, transaction.Action);
    Assert.AreEqual(10, transaction.Quantity);
  }

  [TestMethod]
  [ExpectedException(typeof(ExceedsBalanceException))]
  public void Buy_ExceedsBalance_ShouldThrowException()
  {
    var user = UserFactory.Create(credits: 5);
    var creator = CreatorFactory.Create(value: 50);

    user.Buy(creator, 10);
  }

  // [TestMethod]
  // [ExpectedException(typeof(MaxSharesException))]
  public void Buy_ExceedsMaxShares_ShouldThrowException()
  {
    var user = UserFactory.Create();
    var creator = CreatorFactory.Create();
    user.Buy(creator, User.MAX_SHARES + 1);
  }

  [TestMethod]
  public void Sell_ValidTransaction_ShouldIncreaseCredits()
  {
    var user = UserFactory.Create(credits: 1000);
    var creator = CreatorFactory.Create(value: 10);
    user.Transactions = [
      TransactionFactory.CreateBuy(user, creator, 10),
    ];
    var initialCredits = user.Credits;
    user.Sell(creator, 5);

    Assert.AreEqual(initialCredits + (creator.Value * 5), user.Credits);
    Assert.AreEqual(TransactionAction.Sell, user.Transactions.Last().Action);
    Assert.AreEqual(5, user.Transactions.Last().Quantity);
  }

  [TestMethod]
  [ExpectedException(typeof(ExceedsSharesException))]
  public void Sell_ExceedsOwnedShares_ShouldThrowException()
  {
    var user = UserFactory.Create();
    var creator = CreatorFactory.Create();
    user.Buy(creator, 5);

    user.Sell(creator, 10);
  }

  [TestMethod]
  public void Gamba_ValidLootBox_ShouldReturnResult()
  {
    var user = UserFactory.Create();
    var lootBox = user.AddLootBox();

    var creators = new[] { CreatorFactory.Create() };
    var result = user.Gamba(creators);

    Assert.IsNotNull(result);
    Assert.IsFalse(user.LootBoxes.Contains(lootBox));
  }

  [TestMethod]
  [ExpectedException(typeof(NoLootBoxesException))]
  public void Gamba_NoLootBoxes_ShouldThrowException()
  {
    var user = UserFactory.Create();
    var creators = new[] { CreatorFactory.Create() };

    user.Gamba(creators);
  }

  [TestMethod]
  public void IsAdmin_UserTypeAdmin_ShouldReturnTrue()
  {
    var user = UserFactory.Create();
    user.Type = UserType.Admin;

    Assert.IsTrue(user.IsAdmin());
  }

  [TestMethod]
  public void IsAdmin_UserTypeUser_ShouldReturnFalse()
  {
    var user = UserFactory.Create();
    user.Type = UserType.User;

    Assert.IsFalse(user.IsAdmin());
  }

  [TestMethod]
  public void Create_ValidTwitchUser_ShouldCreateUser()
  {
    var twitchUser = new TwitchUser
    {
      Id = "123",
      DisplayName = "TestUser",
      Login = "testuser",
      AvatarUrl = "https://example.com/avatar.jpg"
    };

    var user = User.Create(twitchUser);

    Assert.AreEqual("TestUser", user.Name);
    Assert.AreEqual("123", user.TwitchId);
    Assert.AreEqual("https://example.com/avatar.jpg", user.AvatarUrl);
  }

  [TestMethod]
  [ExpectedException(typeof(ModelValidationException))]
  public void Create_InvalidTwitchUser_ShouldThrowException()
  {
    var twitchUser = new TwitchUser
    {
      Id = "123",
      DisplayName = "",
      Login = "testuser",
      AvatarUrl = "https://example.com/avatar.jpg"
    };

    User.Create(twitchUser);
  }
}