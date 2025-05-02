using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TTX;
using TTX.Infrastructure.Data;
using TTX.PortfolioMonitor.Provider;
using TTX.PortfolioMonitor.Services;

DotEnv.Load();
ConfigProvider config = new(new ConfigurationBuilder().AddEnvironmentVariables("TTX_").Build());
IServiceProvider services = new ServiceCollection()
    .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(config.GetConnectionString()))
    .AddLogging(options => { options.AddConsole(); })
    .AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(config.GetRedisConnectionString()))
    .AddMediatR(cfg => { cfg.RegisterServicesFromAssemblyContaining<AssemblyReference>(); })
    .AddSingleton<PortfolioMonitorService>()
    .BuildServiceProvider();

var monitor = services.GetRequiredService<PortfolioMonitorService>();
await monitor.Start(config.GetBuffer(), CancellationToken.None);