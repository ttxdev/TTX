using TTX.Models;
using TTX.Queries;
using TTX.Queries.Creators;
using TTX.Queries.Creators.IndexCreators;
using TTX.Tests.Factories;

namespace TTX.Tests.Queries.Creators;

[TestClass]
public class IndexCreatorsQueryTests : ApplicationTests
{
    [TestMethod]
    public async Task LimitTen_ShouldReturnTen()
    {
        int total = 20;
        int limit = 10;

        foreach (int _ in Enumerable.Range(0, total))
        {
            Creator creator = CreatorFactory.Create();
            DbContext.Creators.Add(creator);
        }
        await DbContext.SaveChangesAsync();

        Pagination<Creator> page = await Sender.Send(new IndexCreatorsQuery
        {
            Limit = limit,
            HistoryParams = new HistoryParams()
            {
                Step = TimeStep.ThirtyMinute,
                After = DateTime.UtcNow.AddDays(-1),
            }
        });

        Assert.AreEqual(limit, page.Data.Length);
    }

    [TestMethod]
    public async Task OrderByValueDescending_ShouldReturnOrdered()
    {
        int total = 20;
        foreach (int _ in Enumerable.Range(0, total))
        {
            Creator creator = CreatorFactory.Create();
            DbContext.Creators.Add(creator);
        }
        await DbContext.SaveChangesAsync();

        Pagination<Creator> page = await Sender.Send(new IndexCreatorsQuery
        {
            Limit = total,
            HistoryParams = new HistoryParams()
            {
                Step = TimeStep.ThirtyMinute,
                After = DateTime.UtcNow.AddDays(-1),
            },
            Order = new()
            {
                By = CreatorOrderBy.Value,
                Dir = OrderDirection.Descending
            },
        });

        Assert.AreEqual(total, page.Data.Length);

        for (int i = 1; i < page.Data.Length; i++)
        {
            Assert.IsTrue(page.Data[i - 1].Value >= page.Data[i].Value,
                $"Creator at index {i - 1} has fewer credits than creator at index {i}");
        }
    }

    [TestMethod]
    public async Task OrderByValueAscending_ShouldReturnOrdered()
    {
        int total = 20;
        foreach (int _ in Enumerable.Range(0, total))
        {
            Creator creator = CreatorFactory.Create();
            DbContext.Creators.Add(creator);
        }
        await DbContext.SaveChangesAsync();

        Pagination<Creator> page = await Sender.Send(new IndexCreatorsQuery
        {
            Limit = total,
            HistoryParams = new HistoryParams()
            {
                Step = TimeStep.ThirtyMinute,
                After = DateTime.UtcNow.AddDays(-1),
            },
            Order = new()
            {
                By = CreatorOrderBy.Value,
                Dir = OrderDirection.Ascending
            },
        });

        Assert.AreEqual(total, page.Data.Length);

        for (int i = 1; i < page.Data.Length; i++)
        {
            Assert.IsTrue(page.Data[i - 1].Value <= page.Data[i].Value,
                $"Creator at index {i - 1} has fewer credits than creator at index {i}");
        }
    }

    [TestMethod]
    public async Task SearchByName_ShouldReturnTarget()
    {
        Random random = new Random();
        int total = 20;
        foreach (int _ in Enumerable.Range(0, total))
        {
            DbContext.Creators.Add(CreatorFactory.Create());
        }
        await DbContext.SaveChangesAsync();

        Creator creator = DbContext.Creators.Skip(random.Next(0, total)).First();
        Pagination<Creator> page = await Sender.Send(new IndexCreatorsQuery
        {
            Limit = total,
            HistoryParams = new HistoryParams()
            {
                Step = TimeStep.ThirtyMinute,
                After = DateTime.UtcNow.AddDays(-1),
            },
            Search = creator.Name,
        });

        Creator? target = page.Data.FirstOrDefault();
        Assert.IsNotNull(target);
        Assert.AreEqual(creator.Name, target.Name);
    }
}
