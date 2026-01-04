using Bogus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TTX.App.Data;
using TTX.Tests.App.Factories;
using TTX.Tests.App.Options;
using TTX.App.Events;
using TTX.Domain.Models;
using TTX.App.Interfaces.Platforms;
using TTX.Tests.App.Infrastructure.Platforms;

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

    public static IServiceCollection AddTestInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        foreach (Platform platform in Enum.GetValues<Platform>())
        {
            services.AddKeyedSingleton<IPlatformUserService, PlatformUserService>(platform);
        }

        return services;
    }
}
