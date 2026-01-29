using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TTX.App.Events;
using TTX.App.Interfaces.Platforms;
using TTX.App.Jobs.CreatorValues;
using TTX.App.Jobs.Streams;
using TTX.Domain.Models;
using TTX.Infrastructure.Events.Memory;
using TTX.Infrastructure.Events.Redis;
using TTX.Infrastructure.Options;
using TTX.Infrastructure.Twitch;

namespace TTX.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTtxInfra(this IServiceCollection services, IConfiguration config)
    {
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
            .AddOptions<TwitchChatMonitorOptions>()
            .Bind(config.GetSection("Twitch:Chat"))
            .Services
            .AddKeyedSingleton<IPlatformUserService, TwitchUserService>(Platform.Twitch)
            .AddSingleton<IChatMonitorAdapter, TwitchChatMonitor>()
            .AddKeyedSingleton<IStreamMonitorAdapter, TwitchStreamMonitorAdapter>(Platform.Twitch);

        return services;
    }
}
