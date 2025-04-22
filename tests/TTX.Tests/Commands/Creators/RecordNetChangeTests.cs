using TTX.Commands.Creators.RecordNetChange;
using TTX.Models;
using TTX.Tests.Factories;

namespace TTX.Tests.Commands.Creators;

[TestClass]
public class RecordNetChangeTests : ApplicationTests
{
    [TestMethod]
    public async Task RecordNetChange_ShouldRecordProperly()
    {
        int netChange = 50;
        int value = 100;
        Creator creator = CreatorFactory.Create(value: value);
        DbContext.Creators.Add(creator);
        await DbContext.SaveChangesAsync();

        Vote result = await Sender.Send(new RecordNetChangeCommand
        {
            CreatorSlug = creator.Slug,
            NetChange = netChange
        });

        Assert.AreEqual(value + netChange, result.Creator.Value);
        Assert.AreEqual(value + netChange, result.Value);
    }
}
