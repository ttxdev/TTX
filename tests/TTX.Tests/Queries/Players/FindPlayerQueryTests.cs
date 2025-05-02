using TTX.Commands.Creators.RecordNetChange;
using TTX.Commands.Ordering.PlaceOrder;
using TTX.Commands.Players.CalculatePortfolio;
using TTX.Models;
using TTX.Queries;
using TTX.Queries.Players.FindPlayer;
using TTX.Tests.Factories;

namespace TTX.Tests.Queries.Players;

[TestClass]
public class FindPlayerQueryTests : ApplicationTests
{
    [TestMethod]
    public async Task ValidSlug_ShouldExist()
    {
        var target = PlayerFactory.Create();
        DbContext.Players.Add(target);
        await DbContext.SaveChangesAsync();

        var player = await Sender.Send(new FindPlayerQuery
        {
            Slug = target.Slug,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.ThirtyMinute,
                After = DateTime.UtcNow.AddDays(-1)
            }
        });

        Assert.IsNotNull(player);
    }
       
    [TestMethod]
    public async Task FindPlayer_ShouldReturnValueHistory()
    {
        var target = PlayerFactory.Create(credits: 200);
        var creator = CreatorFactory.Create(value: 50);
        DbContext.Players.Add(target);
        DbContext.Creators.Add(creator);
        await DbContext.SaveChangesAsync();
        await Sender.Send(new PlaceOrderCommand
        {
            Action = TransactionAction.Buy,
            Amount = 1,
            Actor = target.Id,
            Creator = creator.Slug
        });
        await Sender.Send(new CalculatePortfolioCommand
        {
            PlayerId = target.Id
        });

        var player = await Sender.Send(new FindPlayerQuery
        {
            Slug = target.Slug,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.Minute,
                After = DateTime.UtcNow.AddMinutes(-1)
            }
        });

        Assert.IsNotNull(player);
        var portfolio = player.History.FirstOrDefault();
        Assert.IsNotNull(portfolio);
        Assert.AreEqual(50, portfolio.Value);;
    }
}