using TTX.Commands.Creators.RecordNetChange;
using TTX.Tests.Factories;

namespace TTX.Tests.Commands.Creators;

[TestClass]
public class RecordNetChangeTests : ApplicationTests
{
    [TestMethod]
    public async Task RecordNetChange_ShouldRecordProperly()
    {
        var netChange = 50;
        var value = 100;
        var creator = CreatorFactory.Create(value);
        DbContext.Creators.Add(creator);
        await DbContext.SaveChangesAsync();

        var result = await Sender.Send(new RecordNetChangeCommand
        {
            CreatorSlug = creator.Slug,
            NetChange = netChange
        });

        Assert.AreEqual(value + netChange, result.Creator.Value);
        Assert.AreEqual(value + netChange, result.Value);
    }
}