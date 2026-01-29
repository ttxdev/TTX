using Bogus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TTX.Tests.App.Factories;
using TTX.Domain.Models;
using TTX.App.Interfaces.Platforms;
using TTX.Tests.App.Infrastructure.Platforms;
using TTX.App.Events;
using TTX.Infrastructure.Events.Memory;
using TTX.Tests.App.Infrastructure.Streams;
using TTX.App.Jobs.Streams;
using Microsoft.EntityFrameworkCore;
using TTX.App.Data;
using TTX.App.Options;

namespace TTX.Tests.App;

public static class DependencyInjection
{
    public static IServiceCollection AddTestServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<Random>()
            .AddSingleton<Faker>()
            .AddSingleton<NameFactory>()
            .AddSingleton<SlugFactory>()
            .AddSingleton<TickerFactory>()
            .AddSingleton<PlayerFactory>()
            .AddSingleton<CreatorFactory>()
            .AddSingleton<PlatformUserFactory>();
    }

    public static IServiceCollection AddTestInfrastructure(this IServiceCollection services, IConfiguration config, TestContext ctx)
    {
        services
            .ConfigureDbContext<ApplicationDbContext>(opt =>
            {
                opt.EnableDetailedErrors(true);
                opt.EnableSensitiveDataLogging(true);
                if (config.GetValue<DatabaseDriver>("Data") == DatabaseDriver.Postgres)
                {
                    opt.UseNpgsql(config.GetConnectionString("Postgres")!);
                }
                else
                {
                    opt.UseSqlite(config.GetConnectionString("Sqlite") ?? $"Data Source=ttx_test_{ctx.TestName}.db");
                }
            })
            .AddSingleton<IEventDispatcher, MemoryEventHandler>()
            .AddSingleton<IEventReceiver, MemoryEventHandler>()
            .AddSingleton<IStreamMonitorAdapter, TestStreamMonitorAdapter>();

        foreach (Platform platform in Enum.GetValues<Platform>())
        {
            services.AddKeyedSingleton<IPlatformUserService, PlatformUserService>(platform);
        }

        return services;
    }
}
