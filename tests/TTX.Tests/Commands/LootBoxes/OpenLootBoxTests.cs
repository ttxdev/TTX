using TTX.Commands.LootBoxes.OpenLootBox;
using TTX.Models;
using TTX.Tests.Factories;

namespace TTX.Tests.Commands.LootBoxes;

[TestClass]
public class OpenLootBoxTests : ApplicationTests
{
    [TestMethod]
    public async Task OpenLootBox_ShouldProvideResult()
    {
        Player player = PlayerFactory.Create();
        player.AddLootBox();
        foreach (var _ in Enumerable.Range(0, 20))
            DbContext.Creators.Add(CreatorFactory.Create(value: OpenLootBoxHandler.MinValue));
        DbContext.Players.Add(player);
        await DbContext.SaveChangesAsync();

        OpenLootBoxResult result = await Sender.Send(new OpenLootBoxCommand
        {
            ActorSlug = player.Slug
        });

        Assert.IsNotNull(result.Result);
        Assert.AreEqual(20, result.Rarities.Length);
    }
}
