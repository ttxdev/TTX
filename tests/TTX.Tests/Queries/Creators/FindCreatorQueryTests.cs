using TTX.Commands.Creators.RecordNetChange;
using TTX.Queries;
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

    [TestMethod]
    public async Task FindCreator_ShouldReturnValueHistory()
    {
        var target = CreatorFactory.Create(value: 200);
        DbContext.Creators.Add(target);
        await DbContext.SaveChangesAsync();
        await Sender.Send(new RecordNetChangeCommand
        {
            Username = target.Slug,
            NetChange = 50
        });
        await Sender.Send(new RecordNetChangeCommand
        {
            Username = target.Slug,
            NetChange = 25
        });

        var creator = await Sender.Send(new FindCreatorQuery
        {
            Slug = target.Slug,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.Minute,
                After = DateTime.UtcNow.AddMinutes(-1)
            }
        });

        Assert.IsNotNull(creator);
        var vote = creator.History.FirstOrDefault();
        Assert.IsNotNull(vote);
        Assert.AreEqual(275, vote.Value);
    }
}