using TTX.Models;
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
        int total = 20;
        int limit = 10;

        foreach (int _ in Enumerable.Range(0, total))
        {
            Player player = PlayerFactory.Create();
            DbContext.Players.Add(player);
        }
        await DbContext.SaveChangesAsync();

        Pagination<Player> page = await Sender.Send(new IndexPlayersQuery
        {
            Limit = limit,
        });

        Assert.AreEqual(limit, page.Data.Length);
    }

    [TestMethod]
    public async Task OrderByCreditsDescending_ShouldReturnOrdered()
    {
        int total = 20;
        foreach (int _ in Enumerable.Range(0, total))
        {
            Player player = PlayerFactory.Create();
            DbContext.Players.Add(player);
        }
        await DbContext.SaveChangesAsync();

        Pagination<Player> page = await Sender.Send(new IndexPlayersQuery
        {
            Limit = total,
            Order = new()
            {
                By = PlayerOrderBy.Credits,
                Dir = OrderDirection.Descending
            },
        });

        Assert.AreEqual(total, page.Data.Length);

        for (int i = 1; i < page.Data.Length; i++)
        {
            Assert.IsTrue(page.Data[i - 1].Credits >= page.Data[i].Credits,
                $"Player at index {i - 1} has fewer credits than player at index {i}");
        }
    }

    [TestMethod]
    public async Task OrderByCreditsAscending_ShouldReturnOrdered()
    {
        int total = 20;
        foreach (int _ in Enumerable.Range(0, total))
        {
            Player player = PlayerFactory.Create();
            DbContext.Players.Add(player);
        }
        await DbContext.SaveChangesAsync();

        Pagination<Player> page = await Sender.Send(new IndexPlayersQuery
        {
            Limit = total,
            Order = new()
            {
                By = PlayerOrderBy.Credits,
                Dir = OrderDirection.Ascending
            },
        });

        Assert.AreEqual(total, page.Data.Length);

        for (int i = 1; i < page.Data.Length; i++)
        {
            Assert.IsTrue(page.Data[i - 1].Credits <= page.Data[i].Credits,
                $"Player at index {i - 1} has fewer credits than player at index {i}");
        }
    }

    [TestMethod]
    public async Task SearchByName_ShouldReturnTarget()
    {
        Random random = new Random();
        int total = 20;
        foreach (int _ in Enumerable.Range(0, total))
        {
            DbContext.Players.Add(PlayerFactory.Create());
        }
        await DbContext.SaveChangesAsync();

        Player player = DbContext.Players.Skip(random.Next(0, total)).First();
        Pagination<Player> page = await Sender.Send(new IndexPlayersQuery
        {
            Limit = total,
            Search = player.Name,
        });

        Player? target = page.Data.FirstOrDefault();
        Assert.IsNotNull(target);
        Assert.AreEqual(player.Name, target.Name);
    }
}
