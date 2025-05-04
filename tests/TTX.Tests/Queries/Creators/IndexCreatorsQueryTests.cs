using Microsoft.EntityFrameworkCore;
using TTX.Commands.Creators.RecordNetChange;
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
        var total = 20;
        var limit = 10;

        foreach (var _ in Enumerable.Range(0, total))
        {
            var creator = CreatorFactory.Create();
            DbContext.Creators.Add(creator);
        }

        await DbContext.SaveChangesAsync();

        var page = await Sender.Send(new IndexCreatorsQuery
        {
            Limit = limit,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.ThirtyMinute,
                After = DateTime.UtcNow.AddDays(-1)
            }
        });

        Assert.AreEqual(limit, page.Data.Length);
    }

    [TestMethod]
    public async Task OrderByValueDescending_ShouldReturnOrdered()
    {
        var total = 20;
        foreach (var _ in Enumerable.Range(0, total))
        {
            var creator = CreatorFactory.Create();
            DbContext.Creators.Add(creator);
        }

        await DbContext.SaveChangesAsync();

        var page = await Sender.Send(new IndexCreatorsQuery
        {
            Limit = total,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.ThirtyMinute,
                After = DateTime.UtcNow.AddDays(-1)
            },
            Order = new Order<CreatorOrderBy>
            {
                By = CreatorOrderBy.Value,
                Dir = OrderDirection.Descending
            }
        });

        Assert.AreEqual(total, page.Data.Length);

        for (var i = 1; i < page.Data.Length; i++)
            Assert.IsTrue(page.Data[i - 1].Value >= page.Data[i].Value,
                $"Creator at index {i - 1} has fewer credits than creator at index {i}");
    }

    [TestMethod]
    public async Task OrderByValueAscending_ShouldReturnOrdered()
    {
        var total = 20;
        foreach (var _ in Enumerable.Range(0, total))
        {
            var creator = CreatorFactory.Create();
            DbContext.Creators.Add(creator);
        }

        await DbContext.SaveChangesAsync();

        var page = await Sender.Send(new IndexCreatorsQuery
        {
            Limit = total,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.ThirtyMinute,
                After = DateTime.UtcNow.AddDays(-1)
            },
            Order = new Order<CreatorOrderBy>
            {
                By = CreatorOrderBy.Value,
                Dir = OrderDirection.Ascending
            }
        });

        Assert.AreEqual(total, page.Data.Length);

        for (var i = 1; i < page.Data.Length; i++)
            Assert.IsTrue(page.Data[i - 1].Value <= page.Data[i].Value,
                $"Creator at index {i - 1} has fewer credits than creator at index {i}");
    }

    [TestMethod]
    public async Task SearchByName_ShouldReturnTarget()
    {
        var random = new Random();
        var total = 20;
        foreach (var _ in Enumerable.Range(0, total)) DbContext.Creators.Add(CreatorFactory.Create());
        await DbContext.SaveChangesAsync();

        var creator = DbContext.Creators.Skip(random.Next(0, total)).First();
        var page = await Sender.Send(new IndexCreatorsQuery
        {
            Limit = total,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.ThirtyMinute,
                After = DateTime.UtcNow.AddDays(-1)
            },
            Search = creator.Name
        });

        var target = page.Data.FirstOrDefault();
        Assert.IsNotNull(target);
        Assert.AreEqual(creator.Name, target.Name);
    }


    [TestMethod]
    public async Task IndexCreators_ShouldReturnValueHistory()
    {
        int total = 20;
        foreach (var _ in Enumerable.Range(0, total)) DbContext.Creators.Add(CreatorFactory.Create());
        await DbContext.SaveChangesAsync();
        foreach (var c in await DbContext.Creators.ToArrayAsync())
        {
            await Sender.Send(new RecordNetChangeCommand
            {
                CreatorSlug = c.Slug,
                NetChange = Seed.Next(0, 50)
            });
        }

        var page = await Sender.Send(new IndexCreatorsQuery
        {
            Limit = total,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.Minute,
                After = DateTime.UtcNow.AddMinutes(-1)
            },
        });

        foreach (var c in page.Data)
        {
            var vote = c.History.FirstOrDefault();
            Assert.IsNotNull(vote);
            Assert.AreEqual(c.Value, vote.Value);
        }
    }
}