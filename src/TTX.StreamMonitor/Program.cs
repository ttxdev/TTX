using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TTX;
using TTX.Commands.Creators.UpdateStreamStatus;
using TTX.Infrastructure.Data;
using TTX.StreamMonitor.Provider;
using TTX.StreamMonitor.Services;

DotEnv.Load();
ConfigProvider config = new(new ConfigurationBuilder().AddEnvironmentVariables("TTX_").Build());
IServiceProvider services = new ServiceCollection()
    .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(config.GetConnectionString()))
    .AddLogging(options => options.AddConsole())
    .AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(config.GetRedisConnectionString()))
    .AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblyContaining<AssemblyReference>();
    })
    .BuildServiceProvider();

var loggerFactory = services.GetRequiredService<ILoggerFactory>();
TwitchStreamService monitor = new(
    services,
    loggerFactory.CreateLogger<TwitchStreamService>(),
    config.GetTwitchClientId(),
    config.GetTwitchClientSecret()
);
using var scope = services.CreateAsyncScope();
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Creators.ForEachAsync(monitor.AddCreator);
}

await monitor.Start(CancellationToken.None);