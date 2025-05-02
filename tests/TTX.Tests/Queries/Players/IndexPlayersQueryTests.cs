using TTX.Queries;
using TTX.Queries.Players.IndexPlayers;
using TTX.Tests.Factories;

namespace TTX.Tests.Queries.Players;

[TestClass]
public class IndexPlayersQueryTests : ApplicationTests
{
    [TestMethod]
    public async Task LimitTen_ShouldReturnTen()
    {
        var total = 20;
        var limit = 10;

        foreach (var _ in Enumerable.Range(0, total))
        {
            var player = PlayerFactory.Create();
            DbContext.Players.Add(player);
        }

        await DbContext.SaveChangesAsync();

        var page = await Sender.Send(new IndexPlayersQuery
        {
            Limit = limit,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.ThirtyMinute,
                After = DateTime.UtcNow.AddDays(-1)
            },
        });

        Assert.AreEqual(limit, page.Data.Length);
    }

    [TestMethod]
    public async Task OrderByCreditsDescending_ShouldReturnOrdered()
    {
        var total = 20;
        foreach (var _ in Enumerable.Range(0, total))
        {
            var player = PlayerFactory.Create();
            DbContext.Players.Add(player);
        }

        await DbContext.SaveChangesAsync();

        var page = await Sender.Send(new IndexPlayersQuery
        {
            Limit = total,
            HistoryParams = new HistoryParams
            {
                After = DateTimeOffset.Now,
                Step = TimeStep.Day
            },
            Order = new Order<PlayerOrderBy>
            {
                By = PlayerOrderBy.Credits,
                Dir = OrderDirection.Descending
            }
        });

        Assert.AreEqual(total, page.Data.Length);

        for (var i = 1; i < page.Data.Length; i++)
            Assert.IsTrue(page.Data[i - 1].Credits >= page.Data[i].Credits,
                $"Player at index {i - 1} has fewer credits than player at index {i}");
    }

    [TestMethod]
    public async Task OrderByCreditsAscending_ShouldReturnOrdered()
    {
        var total = 20;
        foreach (var _ in Enumerable.Range(0, total))
        {
            var player = PlayerFactory.Create();
            DbContext.Players.Add(player);
        }

        await DbContext.SaveChangesAsync();

        var page = await Sender.Send(new IndexPlayersQuery
        {
            Limit = total,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.ThirtyMinute,
                After = DateTime.UtcNow.AddDays(-1)
            },
            Order = new Order<PlayerOrderBy>
            {
                By = PlayerOrderBy.Credits,
                Dir = OrderDirection.Ascending
            }
        });

        Assert.AreEqual(total, page.Data.Length);

        for (var i = 1; i < page.Data.Length; i++)
            Assert.IsTrue(page.Data[i - 1].Credits <= page.Data[i].Credits,
                $"Player at index {i - 1} has fewer credits than player at index {i}");
    }

    [TestMethod]
    public async Task SearchByName_ShouldReturnTarget()
    {
        var random = new Random();
        var total = 20;
        foreach (var _ in Enumerable.Range(0, total)) DbContext.Players.Add(PlayerFactory.Create());
        await DbContext.SaveChangesAsync();

        var player = DbContext.Players.Skip(random.Next(0, total)).First();
        var page = await Sender.Send(new IndexPlayersQuery
        {
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.ThirtyMinute,
                After = DateTime.UtcNow.AddDays(-1)
            },
            Limit = total,
            Search = player.Name
        });

        var target = page.Data.FirstOrDefault();
        Assert.IsNotNull(target);
        Assert.AreEqual(player.Name, target.Name);
    }
}