using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TTX.App.Data;
using TTX.Tests.App.Factories;
using TTX.App;

namespace TTX.Tests.App.Services;

[TestClass]
public class ServiceTests
{
    protected static IServiceProvider _services = null!;
    protected static CreatorFactory _creatorFactory = null!;
    protected static PlayerFactory _playerFactory = null!;
    protected static PlatformUserFactory _platformUserFactory = null!;
    protected static TickerFactory _tickerFactory = null!;

    [AssemblyInitialize]
    public static async ValueTask Setup(TestContext ctx)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<ServiceTests>()
            .AddEnvironmentVariables("TTX_TESTS_")
            .Build();

        _services = new ServiceCollection()
            .AddTestServices()
            .AddTestInfrastructure(config)
            .AddTtx(config.GetSection("TTX:Core"))
            .BuildServiceProvider();

        _creatorFactory = _services.GetRequiredService<CreatorFactory>();
        _playerFactory = _services.GetRequiredService<PlayerFactory>();
        _platformUserFactory = _services.GetRequiredService<PlatformUserFactory>();
        _tickerFactory = _services.GetRequiredService<TickerFactory>();

        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync(ctx.CancellationToken);
    }

    [AssemblyCleanup]
    public static async ValueTask Cleanup(TestContext ctx)
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureDeletedAsync(ctx.CancellationToken);
    }
}
