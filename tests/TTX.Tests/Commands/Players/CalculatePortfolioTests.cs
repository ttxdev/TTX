using Microsoft.Extensions.DependencyInjection;
using TTX.Commands.Creators.RecordNetChange;
using TTX.Commands.Ordering.PlaceOrder;
using TTX.Commands.Players.CalculatePortfolio;
using TTX.Models;
using TTX.Notifications.Players;
using TTX.Tests.Factories;
using TTX.Tests.Notifications;

namespace TTX.Tests.Commands.Players;

[TestClass]
public class CalculatePortfolioTests : ApplicationTests
{
    [TestMethod]
    public async Task CalculatePortfolio_WithNoTransactions_ShouldReturnCurrentPortfolio()
    {
        var player = PlayerFactory.Create();
        DbContext.Players.Add(player);
        await DbContext.SaveChangesAsync();

        var result = await Sender.Send(new CalculatePortfolioCommand
        {
            PlayerId = player.Id
        });

        Assert.AreEqual(player.Portfolio, result.Value);
        Assert.IsTrue(result.Time > DateTime.MinValue);
    }

    [TestMethod]
    public async Task CalculatePortfolio_WithIncreaseCreatorValue_ShouldIncreasePortfolio()
    {
        const int credits = 100;
        const int value = 50;
        const int increase = 25;
        const int quantity = 1;
        var player = PlayerFactory.Create(credits: credits);
        var creator = CreatorFactory.Create(value: value);
        DbContext.Players.Add(player);
        DbContext.Creators.Add(creator);
        await DbContext.SaveChangesAsync();
        await Sender.Send(new PlaceOrderCommand
        {
            Action = TransactionAction.Buy,
            Amount = quantity,
            Actor = player.Id,
            Creator = creator.Slug
        });
        await Sender.Send(new RecordNetChangeCommand
        {
            CreatorSlug = creator.Slug,
            NetChange = increase
        });

        var result = await Sender.Send(new CalculatePortfolioCommand
        {
            PlayerId = player.Id
        });
        
        var expectedPortfolio = (value * quantity) + increase;
        Assert.AreEqual(expectedPortfolio, player.Portfolio);
        Assert.AreEqual(expectedPortfolio, result.Value);
    }

    [TestMethod]
    public async Task CalculatePortfolio_WithDecreaseCreatorValue_ShouldDecreasePortfolio()
    {
        const int credits = 100;
        const int value = 50;
        const int decrease = -25;
        const int quantity = 1;
        var player = PlayerFactory.Create(credits: credits);
        var creator = CreatorFactory.Create(value: value);
        DbContext.Players.Add(player);
        DbContext.Creators.Add(creator);
        await DbContext.SaveChangesAsync();
        await Sender.Send(new PlaceOrderCommand
        {
            Action = TransactionAction.Buy,
            Amount = quantity,
            Creator = creator.Slug,
            Actor = player.Id
        });
        
        await Sender.Send(new RecordNetChangeCommand
        {
            CreatorSlug = creator.Slug,
            NetChange = decrease
        });
        
        var result = await Sender.Send(new CalculatePortfolioCommand
        {
            PlayerId = player.Id
        });
        
        var expectedPortfolio = (value * quantity) + decrease;
        Assert.AreEqual(expectedPortfolio, player.Portfolio);
        Assert.AreEqual(expectedPortfolio, result.Value);
    }
}
