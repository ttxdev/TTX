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
        Assert.AreEqual(total, page.Total);
    }
}
