using TTX.Tests.Factories;
using TTX.Commands.Creators.CreatorOptOuts;

namespace TTX.Tests.Commands.Creators;

[TestClass]
public class CreatorOptOutTest : ApplicationTests
{
    [TestMethod]
    public async Task ApproveApplication_ShouldPass()
    {
        var creator = CreatorFactory.Create();
        DbContext.Creators.Add(creator);
        await DbContext.SaveChangesAsync();

        var result = await Sender.Send(new CreatorOptOutCommand
        {
           Username = creator.Slug
        });

        Assert.IsNotNull(result);
        Assert.AreEqual(creator.TwitchId, result.TwitchId);
        Assert.IsFalse(DbContext.Creators.Any(c=> c.TwitchId == creator.TwitchId));
    }
}