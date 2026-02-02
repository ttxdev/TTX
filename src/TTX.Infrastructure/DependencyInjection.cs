using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TTX.App.Data;
using TTX.App.Events;
using TTX.App.Interfaces.Platforms;
using TTX.App.Jobs.CreatorValues;
using TTX.App.Jobs.Streams;
using TTX.Domain.Models;
using TTX.Infrastructure.Events.Memory;
using TTX.Infrastructure.Events.Redis;
using TTX.Infrastructure.Options;
using TTX.Infrastructure.Twitch;
#if BOT_EXISTS
using TTX.Bot;
#endif

namespace TTX.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTtxInfra(this IServiceCollection services, IConfiguration config)
    {
        services.ConfigureDbContext<ApplicationDbContext>((opt) =>
            {
                if (config.GetValue<DatabaseDriver>("Data") == DatabaseDriver.Postgres)
                {
                    opt.UseNpgsql(config.GetConnectionString("Postgres")!);
                }
                else
                {
                    opt.UseSqlite(config.GetConnectionString("Sqlite")!);
                }
            });

        switch (config.GetValue<EventDriver>("Events"))
        {
            case EventDriver.Redis:
                services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!));
                services.AddSingleton<IEventDispatcher, RedisEventDispatcher>();
                services.AddSingleton<IEventReceiver, RedisEventReceiver>();
                break;
            case EventDriver.Memory:
            default:
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
            .AddKeyedSingleton<IPlatformUserService, TwitchUserService>(Platform.Twitch)
            .AddSingleton<IStreamMonitorAdapter, TwitchStreamMonitorAdapter>();

#if BOT_EXISTS
        services.AddValueMonitor();
#else
        services.AddOptions<TwitchChatMonitorOptions>()
            .Bind(config.GetSection("Twitch:Chat"))
            .Services
            .AddSingleton<IChatMonitorAdapter, SimpleTwitchChatMonitor>();
#endif

        return services;
    }
}
