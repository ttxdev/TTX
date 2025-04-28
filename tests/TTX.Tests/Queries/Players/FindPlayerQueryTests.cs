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
            Slug = target.Slug
        });

        Assert.IsNotNull(player);
    }
}