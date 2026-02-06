using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TTX.App.Data;
using TTX.App.Data.Repositories;
using TTX.App.Dto.Pagination;
using TTX.App.Dto.Players;
using TTX.App.Dto.Portfolio;
using TTX.App.Services.Players;
using TTX.Domain.Models;

namespace TTX.Tests.App.Services.Players;

[TestClass]
public class IndexPlayersTests : ServiceTests
{
    [TestInitialize]
    public void CheckSkip()
    {
        using IServiceScope scope = _services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!dbContext.Database.IsNpgsql())
        {
            // NOTE(dylhack): Due to PortfolioService#GetHistoryFor having a hard requirement on Timescale DB
            //                we are unable to resolve the player portfolio history in any of these tests causing
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
        PlayerService playerService = scope.ServiceProvider.GetRequiredService<PlayerService>();
        foreach (int _ in Enumerable.Range(0, total))
        {
            Player player = _playerFactory.Create();
            dbContext.Players.Add(player);
        }
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);

        PaginationDto<PlayerDto> page = await playerService.Index(new IndexPlayersRequest
        {
            Limit = limit,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.ThirtyMinute,
                Before = TimeSpan.FromDays(1)
            },
        });

        Assert.HasCount(limit, page.Data);
    }

    [TestMethod]
    public async Task OrderByCreditsDescending_ShouldReturnOrdered()
    {
        const int total = 20;
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        PlayerService playerService = scope.ServiceProvider.GetRequiredService<PlayerService>();
        foreach (int _ in Enumerable.Range(0, total))
        {
            Player player = _playerFactory.Create();
            dbContext.Players.Add(player);
        }
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);

        PaginationDto<PlayerDto> page = await playerService.Index(new IndexPlayersRequest
        {
            Limit = total,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.Day,
                Before = TimeSpan.FromDays(1)
            },
            Order = new Order<PlayerOrderBy>
            {
                By = PlayerOrderBy.Credits,
                Dir = OrderDirection.Descending
            }
        });

        Assert.HasCount(total, page.Data);
        for (int i = 1; i < page.Data.Length; i++)
        {
            Assert.IsGreaterThanOrEqualTo(page.Data[i].Credits,
page.Data[i - 1].Credits, $"Player at index {i - 1} has fewer credits than player at index {i}");
        }
    }

    [TestMethod]
    public async Task OrderByCreditsAscending_ShouldReturnOrdered()
    {
        const int total = 100;
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        PlayerService playerService = scope.ServiceProvider.GetRequiredService<PlayerService>();
        foreach (int _ in Enumerable.Range(0, total))
        {
            Player player = _playerFactory.Create();
            dbContext.Players.Add(player);
        }
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);

        PaginationDto<PlayerDto> page = await playerService.Index(new IndexPlayersRequest
        {
            Limit = total,
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.ThirtyMinute,
                Before = TimeSpan.FromDays(1)
            },
            Order = new Order<PlayerOrderBy>
            {
                By = PlayerOrderBy.Credits,
                Dir = OrderDirection.Ascending
            }
        });

        Assert.HasCount(total, page.Data);

        for (int i = 1; i < page.Data.Length; i++)
        {
            Assert.IsLessThanOrEqualTo(page.Data[i].Credits,
page.Data[i - 1].Credits, $"Player at index {i - 1} has fewer credits than player at index {i}");
        }
    }

    [TestMethod]
    public async Task SearchByName_ShouldReturnTarget()
    {
        const int total = 20;
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        PlayerService playerService = scope.ServiceProvider.GetRequiredService<PlayerService>();
        Random random = scope.ServiceProvider.GetRequiredService<Random>();
        foreach (int _ in Enumerable.Range(0, total))
        {
            dbContext.Players.Add(_playerFactory.Create());
        }
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);
        Player player = dbContext.Players.Skip(random.Next(0, total)).First();
        PaginationDto<PlayerDto> page = await playerService.Index(new IndexPlayersRequest
        {
            HistoryParams = new HistoryParams
            {
                Step = TimeStep.ThirtyMinute,
                Before = TimeSpan.FromDays(1)
            },
            Limit = total,
            Search = player.Name
        });

        Assert.IsTrue(page.Data.Any(p => p.Id == player.Id));
    }

    [TestMethod]
    public async Task ValidSlug_ShouldExist()
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        PlayerService playerService = scope.ServiceProvider.GetRequiredService<PlayerService>();
        Player target = _playerFactory.Create();
        dbContext.Players.Add(target);
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);

        PlayerDto? player = await playerService.Find(target.Slug, new HistoryParams
        {
            Step = TimeStep.ThirtyMinute,
            Before = TimeSpan.FromDays(1)
        });

        Assert.IsNotNull(player);
    }

    [TestMethod]
    public async Task FindPlayer_ShouldReturnValueHistory()
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        PortfolioRepository portfolioRepository = scope.ServiceProvider.GetRequiredService<PortfolioRepository>();
        PlayerService playerService = scope.ServiceProvider.GetRequiredService<PlayerService>();
        Player target = _playerFactory.Create(credits: 200);
        Creator creator = _creatorFactory.Create(value: 50);
        dbContext.Players.Add(target);
        dbContext.Creators.Add(creator);
        target.Give(creator);
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);
        await portfolioRepository.StoreSnapshot(target.TakePortfolioSnapshot());
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);

        PlayerDto? player = await playerService.Find(target.Slug, new HistoryParams
        {
            Step = TimeStep.Minute,
            Before = TimeSpan.FromMinutes(1)
        });

        Assert.IsNotNull(player);
        PortfolioSnapshotDto? portfolio = player.History.LastOrDefault();
        Assert.IsNotNull(portfolio);
        Assert.AreEqual(50, portfolio.Value);
    }
}
