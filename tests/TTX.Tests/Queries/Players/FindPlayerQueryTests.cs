using TTX.Models;
using TTX.Queries.Players.FindPlayer;
using TTX.Tests.Factories;

namespace TTX.Tests.Queries.Players;

[TestClass]
public class FindPlayerQueryTests : ApplicationTests
{
    [TestMethod]
    public async Task ValidSlug_ShouldExist()
    {
        Player target = PlayerFactory.Create();
        DbContext.Players.Add(target);
        await DbContext.SaveChangesAsync();

        Player? player = await Sender.Send(new FindPlayerQuery
        {
            Slug = target.Slug,
        });

        Assert.IsNotNull(player);
    }
}
