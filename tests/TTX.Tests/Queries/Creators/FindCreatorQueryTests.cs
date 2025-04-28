using TTX.Queries.Creators;
using TTX.Queries.Creators.FindCreator;
using TTX.Tests.Factories;

namespace TTX.Tests.Queries.Creators;

[TestClass]
public class FindCreatorQueryTests : ApplicationTests
{
    [TestMethod]
    public async Task ValidSlug_ShouldExist()
    {
        var target = CreatorFactory.Create();
        DbContext.Creators.Add(target);
        await DbContext.SaveChangesAsync();

        var creator = await Sender.Send(new FindCreatorQuery
        {
            Slug = target.Slug,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.ThirtyMinute,
                After = DateTime.UtcNow.AddDays(-1)
            }
        });

        Assert.IsNotNull(creator);
    }
}