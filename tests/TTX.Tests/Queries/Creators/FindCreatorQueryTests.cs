namespace TTX.Tests.Queries.Creators;

[TestClass]
public class FindCreatorQueryTests : ApplicationTests
{
    // TODO(dylhack): Finish creator query tests once we utilize timescale in our test runners
    //[TestMethod]
    //public async Task ValidSlug_ShouldExist()
    //{
    //    Creator target = CreatorFactory.Create();
    //    DbContext.Creators.Add(target);
    //    await DbContext.SaveChangesAsync();

    //    Creator? creator = await Sender.Send(new FindCreatorQuery
    //    {
    //        Slug = target.Slug,
    //        HistoryParams = new HistoryParams
    //        {
    //            Step = TimeStep.ThirtyMinute,
    //            After = DateTime.UtcNow.AddDays(-1),
    //        }
    //    });

    //    Assert.IsNotNull(creator);
    //}
}
