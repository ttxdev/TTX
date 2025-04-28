using TTX.Queries;
using TTX.Queries.Creators;
using TTX.Queries.Creators.FindCreator;
using TTX.Queries.Creators.IndexCreators;
using TTX.Tests.Factories;
using TTX.Models;

namespace TTX.Tests.Queries.Creators;

[TestClass]
public class FindCreatorQueryTests : ApplicationTests
{
    [TestMethod]
    public async Task ValidSlug_ShouldExist()
    {
        Creator target = CreatorFactory.Create();
        DbContext.Creators.Add(target);
        await DbContext.SaveChangesAsync();

        Creator? creator = await Sender.Send(new FindCreatorQuery
        {
            Slug = target.Slug,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.ThirtyMinute,
                After = DateTime.UtcNow.AddDays(-1),
            }
        });

        Assert.IsNotNull(creator);
    }
}
