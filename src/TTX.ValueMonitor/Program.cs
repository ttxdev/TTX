using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TTX.Commands.Creators.UpdateStreamStatus;
using TTX.Infrastructure.Data;
using TTX.ValueMonitor.Provider;
using TTX.ValueMonitor.Services;

DotEnv.Load();
IConfigProvider config = new ConfigProvider(new ConfigurationBuilder().AddEnvironmentVariables("TTX_").Build());
IServiceProvider services = new ServiceCollection()
    .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(config.GetConnectionString()))
    .AddLogging(options => { options.AddConsole(); })
    .AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(config.GetRedisConnectionString()))
    .AddMediatR(cfg => { cfg.RegisterServicesFromAssemblyContaining<UpdateStreamStatusHandler>(); })
    .BuildServiceProvider();

var loggerFactory = services.GetRequiredService<ILoggerFactory>();
TwitchBotService bot = new(services, loggerFactory.CreateLogger<TwitchBotService>());
using var scope = services.CreateAsyncScope();
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Creators.ForEachAsync(bot.AddCreator);
}

await bot.Start(CancellationToken.None);