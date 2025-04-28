using TTX.Commands.LootBoxes.OpenLootBox;
using TTX.Tests.Factories;

namespace TTX.Tests.Commands.LootBoxes;

[TestClass]
public class OpenLootBoxTests : ApplicationTests
{
    [TestMethod]
    public async Task OpenLootBox_ShouldProvideResult()
    {
        var player = PlayerFactory.Create();
        player.AddLootBox();
        foreach (var _ in Enumerable.Range(0, 20))
            DbContext.Creators.Add(CreatorFactory.Create(OpenLootBoxHandler.MinValue));
        DbContext.Players.Add(player);
        await DbContext.SaveChangesAsync();

        var result = await Sender.Send(new OpenLootBoxCommand
        {
            ActorSlug = player.Slug
        });

        Assert.IsNotNull(result.Result);
    }
}