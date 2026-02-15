using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TTX.App.Data;
using TTX.App.Events;
using TTX.App.Interfaces.Platforms;
using TTX.App.Jobs.Streams;
using TTX.Domain.Models;
using TTX.Infrastructure.Events.Redis;
using TTX.Infrastructure.Options;
using TTX.Infrastructure.Twitch;
using TTX.Infrastructure.Twitch.Chat;
using TTX.Infrastructure.Data.Repositories;
using TTX.App.Interfaces.CreatorValue;
using TTX.Infrastructure.Services;
using TTX.App.Interfaces.Chat;
using TTX.App.Interfaces.Data.CreatorValue;


#if TTX_PRIVATE_EXISTS
using TTX.Private;
#else
#endif

namespace TTX.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTtxInfra(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddLogging()
            .ConfigureDbContext<ApplicationDbContext>((opt) =>
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

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!))
            .AddSingleton<IEventDispatcher, RedisEventDispatcher>()
            .AddSingleton<IEventReceiver, RedisEventReceiver>()
            .AddSingleton<ICreatorStatsRepository, CreatorStatsRepository>();

        services
            .AddOptions<TwitchOAuthOptions>()
            .Bind(config.GetSection("Twitch:OAuth"))
            .Services
            // Twitch OAuth & Users
            .AddKeyedSingleton<IPlatformUserService, TwitchUserService>(Platform.Twitch)
            // Twitch Streams
            .AddSingleton<IStreamMonitorAdapter, TwitchStreamMonitorAdapter>()
            // Twitch Chat
            .AddKeyedScoped<IChatMonitorAdapter, TwitchChatAdapter>(Platform.Twitch)
            .AddScoped<BotContainer>()
            .AddScoped<TwitchBot>();

#if TTX_PRIVATE_EXISTS
        services.AddPrivateServices(config);
#else
        services.AddSingleton<IMessageAnalyzer, MessageAnalyzer>();
        services.AddSingleton<IStatsProcessor, StatsProcessor>();
#endif


        return services;
    }
}
