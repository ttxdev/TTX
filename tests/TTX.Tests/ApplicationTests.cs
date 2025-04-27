using Bogus;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TTX.Commands.Creators.OnboardTwitchCreator;
using TTX.Commands.Creators.RecordNetChange;
using TTX.Commands.Creators.UpdateStreamStatus;
using TTX.Commands.LootBoxes.OpenLootBox;
using TTX.Commands.Ordering.PlaceOrder;
using TTX.Infrastructure.Data;
using TTX.Queries.Creators.FindCreator;
using TTX.Queries.Creators.IndexCreators;
using TTX.Queries.Creators.PullLatestHistory;
using TTX.Queries.Players.FindPlayer;
using TTX.Queries.Players.IndexPlayers;

namespace TTX.Tests;

public abstract class ApplicationTests
{
    protected IServiceProvider ServiceProvider { get; private set; } = null!;
    protected ISender Sender { get; private set; } = null!;
    protected ApplicationDbContext DbContext { get; private set; } = null!;

    [TestInitialize]
    public void Setup()
    {
        int seed = new Random().Next(1, 1000);
        Randomizer.Seed = new Random(seed);
        Console.WriteLine($"Using seed {seed}");

        ServiceProvider = new ServiceCollection()
            .AddLogging()
            .AddDbContext<ApplicationDbContext>(options =>
            {
                var dbPath = Path.GetTempFileName();
                Console.WriteLine($"Initialize SQLite Database {dbPath}");
                options.UseSqlite($"Data Source={dbPath}");
            })
            .AddMediatR(cfg =>
            {
                // creator queries
                cfg.RegisterServicesFromAssemblyContaining<IndexCreatorsHandler>();
                cfg.RegisterServicesFromAssemblyContaining<FindCreatorHandler>();
                cfg.RegisterServicesFromAssemblyContaining<PullLatestHistoryHandler>();
                // player queries
                cfg.RegisterServicesFromAssemblyContaining<FindPlayerHandler>();
                cfg.RegisterServicesFromAssemblyContaining<IndexPlayersHandler>();

                // creator commands
                cfg.RegisterServicesFromAssemblyContaining<OnboardTwitchCreatorHandler>();
                cfg.RegisterServicesFromAssemblyContaining<RecordNetChangeHandler>();
                cfg.RegisterServicesFromAssemblyContaining<UpdateStreamStatusHandler>();
                // lootbox commands
                cfg.RegisterServicesFromAssemblyContaining<OpenLootBoxCommand>();
                // ordering commands
                cfg.RegisterServicesFromAssemblyContaining<PlaceOrderHandler>();
            })
            .BuildServiceProvider();

        DbContext = ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Sender = ServiceProvider.GetRequiredService<ISender>();
        DbContext.Database.EnsureCreated();
    }

    [TestCleanup]
    public virtual void TestCleanup()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
    }
}
