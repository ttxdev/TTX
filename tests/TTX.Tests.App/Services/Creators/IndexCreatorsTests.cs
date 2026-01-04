using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TTX.App.Data;
using TTX.App.Dto.Creators;
using TTX.App.Dto.Pagination;
using TTX.App.Dto.Portfolio;
using TTX.App.Services.Creators;
using TTX.Domain.Models;

namespace TTX.Tests.App.Services.Creators;

[TestClass]
public class IndexCreatorsTests : ServiceTests
{
    public virtual TestContext TestContext { get; set; }

    [TestInitialize]
    public void Setup()
    {
        using IServiceScope scope = _services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!dbContext.Database.IsNpgsql())
        {
            // NOTE(dylhack): Due to PortfolioService#GetHistoryFor having a hard requirement on Timescale DB
            //                we are unable to resolve the creator value history in any of these tests causing
            //                them to fail.
            Assert.Inconclusive("This test is only applicable for Timescale");
        }
    }

    [TestMethod]
    public async Task LimitTen_ShouldReturnTen()
    {
        const int total = 20;
        const int limit = 10;
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        CreatorService creatorService = scope.ServiceProvider.GetRequiredService<CreatorService>();
        foreach (int _ in Enumerable.Range(0, total))
        {
            dbContext.Creators.Add(_creatorFactory.Create());
        }
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);

        PaginationDto<CreatorDto> page = await creatorService.Index(new IndexCreatorsRequest
        {
            Limit = limit,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.ThirtyMinute,
                After = DateTime.UtcNow.AddDays(-1)
            }
        });

        Assert.HasCount(limit, page.Data);
    }

    [TestMethod]
    public async Task OrderByValueDescending_ShouldReturnOrdered()
    {
        const int total = 20;
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        CreatorService creatorService = scope.ServiceProvider.GetRequiredService<CreatorService>();
        foreach (int _ in Enumerable.Range(0, total))
        {
            dbContext.Creators.Add(_creatorFactory.Create());
        }

        await dbContext.SaveChangesAsync(TestContext.CancellationToken);

        PaginationDto<CreatorDto> page = await creatorService.Index(new IndexCreatorsRequest
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

        Assert.HasCount(total, page.Data);
        for (int i = 1; i < page.Data.Length; i++)
        {
            Assert.IsGreaterThanOrEqualTo(page.Data[i].Value,
page.Data[i - 1].Value, $"Creator at index {i - 1} has fewer credits than creator at index {i}");
        }
    }

    [TestMethod]
    public async Task OrderByValueAscending_ShouldReturnOrdered()
    {
        const int total = 20;
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        CreatorService creatorService = scope.ServiceProvider.GetRequiredService<CreatorService>();
        foreach (int _ in Enumerable.Range(0, total))
        {
            dbContext.Creators.Add(_creatorFactory.Create());
        }

        await dbContext.SaveChangesAsync(TestContext.CancellationToken);

        PaginationDto<CreatorDto> page = await creatorService.Index(new IndexCreatorsRequest
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

        Assert.HasCount(total, page.Data);
        for (int i = 1; i < page.Data.Length; i++)
        {
            Assert.IsLessThanOrEqualTo(page.Data[i].Value,
page.Data[i - 1].Value, $"Creator at index {i - 1} has fewer credits than creator at index {i}");
        }
    }

    [TestMethod]
    public async Task SearchByName_ShouldReturnTarget()
    {
        const int total = 20;
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        CreatorService creatorService = scope.ServiceProvider.GetRequiredService<CreatorService>();
        Random random = scope.ServiceProvider.GetRequiredService<Random>();
        foreach (int _ in Enumerable.Range(0, total))
        {
            dbContext.Creators.Add(_creatorFactory.Create());
        }
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);

        Creator creator = dbContext.Creators.Skip(random.Next(0, total)).First();
        PaginationDto<CreatorDto> page = await creatorService.Index(new IndexCreatorsRequest
        {
            Limit = 5,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.ThirtyMinute,
                After = DateTime.UtcNow.AddDays(-1)
            },
            Search = creator.Name
        });

        Assert.IsTrue(page.Data.Any(c => c.Id == creator.Id));
    }


    [TestMethod]
    public async Task IndexCreators_ShouldReturnValueHistory()
    {
        const int total = 20;
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        CreatorService creatorService = scope.ServiceProvider.GetRequiredService<CreatorService>();
        Random random = scope.ServiceProvider.GetRequiredService<Random>();
        foreach (int _ in Enumerable.Range(0, total))
        {
            Creator creator = _creatorFactory.Create();
            dbContext.Creators.Add(creator);
            creator.ApplyNetChange(random.Next(200));
        }
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);

        PaginationDto<CreatorDto> page = await creatorService.Index(new IndexCreatorsRequest
        {
            Limit = total,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.Minute,
                After = DateTime.UtcNow.AddMinutes(-1)
            },
        });

        Assert.IsTrue(page.Data.Any(c => c.History.Length > 0));
    }

    [TestMethod]
    public async Task ValidSlug_ShouldExist()
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        CreatorService creatorService = scope.ServiceProvider.GetRequiredService<CreatorService>();
        Creator target = _creatorFactory.Create();
        dbContext.Creators.Add(target);
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);

        CreatorDto? creator = await creatorService.Find(target.Slug, new HistoryParams
        {
            Step = TimeStep.ThirtyMinute,
            After = DateTime.UtcNow.AddDays(-1)
        });

        Assert.IsNotNull(creator);
    }

    [TestMethod]
    public async Task FindCreator_ShouldReturnValueHistory()
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        CreatorService creatorService = scope.ServiceProvider.GetRequiredService<CreatorService>();
        Creator target = _creatorFactory.Create(value: 200);
        dbContext.Creators.Add(target);
        target.ApplyNetChange(50);
        target.ApplyNetChange(25);
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);

        CreatorDto? creator = await creatorService.Find(target.Slug, new HistoryParams
        {
            Step = TimeStep.Minute,
            After = DateTime.UtcNow.AddMinutes(-1)
        });

        Assert.IsNotNull(creator);
        VoteDto? vote = creator.History.FirstOrDefault();
        Assert.IsNotNull(vote);
        Assert.AreEqual(275, vote.Value);
    }
}
