using Bogus;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TTX.Infrastructure.Data;
using TTX.Interfaces.Twitch;
using TTX.Tests.Infrastructure.Twitch;
using TTX.Tests.Notifications;
using dotenv.net;
using TTX.Api.Interfaces;
using TTX.Api.Services;

namespace TTX.Tests;

public abstract class ApplicationTests
{
    protected IServiceProvider ServiceProvider { get; private set; } = null!;
    protected ISender Sender { get; private set; } = null!;
    protected ApplicationDbContext DbContext { get; private set; } = null!;
    protected Random Seed { get; private set; } = null!;

    [TestInitialize]
    public void Setup()
    {
        DotEnv.Load();

        var seedI = new Random().Next(1, 1000);
        Seed = new Random(seedI);
        Randomizer.Seed = Seed;
        Console.WriteLine($"Using seed {seedI}");

        ServiceProvider = new ServiceCollection()
            .AddLogging()
            .AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(Environment.GetEnvironmentVariable("TTX_CONNECTION_STRING"));
            })
            .AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<AssemblyReference>();
                cfg.RegisterServicesFromAssemblyContaining<ApplicationTests>();
            })
            .AddSingleton(Seed)
            .AddSingleton<ISessionService, SessionService>()
            .AddSingleton<CreateCreatorNotificationHandler>()
            .AddSingleton<CreatePlayerNotificationHandler>()
            .AddSingleton<UpdateCreatorValueNotificationHandler>()
            .AddSingleton<OpenLootBoxNotificationHandler>()
            .AddSingleton<CreateTransactionNotificationHandler>()
            .AddSingleton<UpdatePlayerPortfolioNotificationHandler>()
            .AddSingleton<ITwitchAuthService, TwitchAuthService>(_ => new TwitchAuthService())
            .AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("TTX_REDIS_URL")!))
            .BuildServiceProvider();

        DbContext = ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Sender = ServiceProvider.GetRequiredService<ISender>();
        TestNotificationHandler.Notifications.Clear();
        DbContext.Database.EnsureDeleted();
        DbContext.Database.Migrate();
    }

    [TestCleanup]
    public virtual void TestCleanup()
    {
        DbContext.Dispose();
    }
}
