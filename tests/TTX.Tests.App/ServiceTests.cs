using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TTX.Tests.App.Factories;
using TTX.App;
using TTX.App.Data;
using TTX.App.Jobs.Streams;
using Microsoft.EntityFrameworkCore;

namespace TTX.Tests.App;

[TestClass]
public class ServiceTests
{
    protected static IServiceProvider _services = null!;
    protected static CreatorFactory _creatorFactory = null!;
    protected static PlayerFactory _playerFactory = null!;
    protected static PlatformUserFactory _platformUserFactory = null!;
    protected static TickerFactory _tickerFactory = null!;

    public virtual TestContext TestContext { get; set; } = null!;

    [AssemblyInitialize]
    public static void Setup(TestContext testContext)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<ServiceTests>()
            .AddEnvironmentVariables()
            .Build();

        _services = new ServiceCollection()
            .AddTtx(config.GetSection("TTX"))
            .AddTestServices()
            .AddTestInfrastructure(config.GetSection("TTX:Infrastructure"), testContext)
            .AddSingleton<StreamMonitorJob>()
            .BuildServiceProvider();

        _creatorFactory = _services.GetRequiredService<CreatorFactory>();
        _playerFactory = _services.GetRequiredService<PlayerFactory>();
        _platformUserFactory = _services.GetRequiredService<PlatformUserFactory>();
        _tickerFactory = _services.GetRequiredService<TickerFactory>();

        ApplicationDbContext dbContext = _services.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
    }

    [AssemblyCleanup]
    public static void Cleanup(TestContext context)
    {
        ApplicationDbContext dbContext = _services.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureDeleted();
    }
}
