using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TTX.App.Events;
using TTX.App.Interfaces.Platforms;
using TTX.App.Jobs.CreatorValues;
using TTX.App.Jobs.Streams;
using TTX.App.Repositories;
using TTX.Domain.Models;
using TTX.Infrastructure.Data;
using TTX.Infrastructure.Data.Repositories;
using TTX.Infrastructure.Events.Memory;
using TTX.Infrastructure.Events.Redis;
using TTX.Infrastructure.Options;
using TTX.Infrastructure.Twitch;

namespace TTX.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTtxInfra(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddDbContext<ApplicationDbContext>((opt) =>
            {
                opt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                if (config.GetValue<DatabaseDriver>("Data") == DatabaseDriver.Postgres)
                {
                    opt.UseNpgsql(config.GetConnectionString("Postgres")!);
                }
                else
                {
                    opt.UseSqlite(config.GetConnectionString("Sqlite")!);
                }
            })
            .AddScoped<PortfolioRepository>()
            .AddScoped<ICreatorRepository, CreatorRepository>()
            .AddScoped<IPlayerRepository, PlayerRepository>()
            .AddScoped<ITransactionRepository, TransactionRepository>();

        switch (config.GetValue<EventDriver>("Events"))
        {
            case EventDriver.Redis:
                services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!));
                services.AddSingleton<IEventDispatcher, RedisEventDispatcher>();
                services.AddSingleton<IEventReceiver, RedisEventReceiver>();
                break;
            default:
            case EventDriver.Memory:
                services.AddSingleton<IEventDispatcher, MemoryEventHandler>();
                services.AddSingleton<IEventReceiver, MemoryEventHandler>();
                break;
        }

        services
            .AddOptions<TwitchOAuthOptions>()
            .Bind(config.GetSection("Twitch:OAuth"))
            .Services
            .AddOptions<TwitchStreamMonitorOptions>()
            .Bind(config.GetSection("Twitch:Streams"))
            .Services
            .AddOptions<TwitchChatMonitorOptions>()
            .Bind(config.GetSection("Twitch:Chat"))
            .Services
            .AddKeyedSingleton<IPlatformUserService, TwitchUserService>(Platform.Twitch)
            .AddSingleton<IChatMonitorAdapter, TwitchChatMonitor>()
            .AddKeyedSingleton<IStreamMonitorAdapter, TwitchStreamMonitorAdapter>(Platform.Twitch);

        return services;
    }
}
