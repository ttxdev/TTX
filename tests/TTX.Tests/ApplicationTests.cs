using Bogus;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TTX.Infrastructure.Data;
using TTX.Interfaces.Twitch;
using TTX.Notifications.Players;
using TTX.Tests.Infrastructure.Twitch;
using TTX.Tests.Notifications;
using INotification = TTX.Notifications.INotification;

namespace TTX.Tests;

public abstract class ApplicationTests
{
    protected IServiceProvider ServiceProvider { get; private set; } = null!;
    protected ISender Sender { get; private set; } = null!;
    protected ApplicationDbContext DbContext { get; private set; } = null!;

    [TestInitialize]
    public void Setup()
    {
        var seedI = new Random().Next(1, 1000);
        var seed = new Random(seedI);
        Randomizer.Seed = seed;
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
            .AddSingleton(seed)
            .AddSingleton<CreateCreatorNotificationHandler>()
            .AddSingleton<CreatePlayerNotificationHandler>()
            .AddSingleton<UpdateCreatorValueNotificationHandler>()
            .AddSingleton<OpenLootBoxNotificationHandler>()
            .AddSingleton<ITwitchAuthService, TwitchAuthService>(_ => new TwitchAuthService())
            .AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("TTX_REDIS_URL")!))
            .BuildServiceProvider();

        DbContext = ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Sender = ServiceProvider.GetRequiredService<ISender>();
        DbContext.Database.EnsureDeleted();
        DbContext.Database.EnsureCreated();
    }

    [TestCleanup]
    public virtual void TestCleanup()
    {
        DbContext.Dispose();
    }
}
