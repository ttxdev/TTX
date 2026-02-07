using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TTX.App.Data;
using TTX.App.Data.Repositories;
using TTX.App.Jobs.CreatorValues;
using TTX.App.Jobs.Portfolios;
using TTX.App.Jobs.Streams;
using TTX.App.Options;
using TTX.App.Services.Creators;
using TTX.App.Services.LootBoxes;
using TTX.App.Services.Players;
using TTX.App.Services.Transactions;

namespace TTX.App;

public static class DependencyInjection
{
    public static IServiceCollection AddTtx(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddLogging()
            .AddSingleton<Random>((services) =>
                {
                    RandomOptions? options = configuration.GetValue<RandomOptions>("Random");
                    if (options is not null && options.Seed.HasValue)
                    {
                        return new Random(options.Seed.Value);
                    }

                    return new Random();
                })
                // Data
                .AddDbContext<ApplicationDbContext>()
                .AddScoped<PortfolioRepository>()
            // Services
            .AddScoped<CreatorService>()
            .AddScoped<LootBoxService>()
            .AddScoped<TransactionService>()
            .AddScoped<PlayerService>();
    }

    public static IServiceCollection AddTtxJobs(this IServiceCollection services, IConfiguration configuration)
    {
        string[]? enabled = configuration.GetSection("Enabled").Get<string[]?>();

        if (enabled is null || enabled.Contains(nameof(CalculatePlayerPortfolioJob)))
        {
            services
                .AddOptions<CalculatePlayerPortfolioOptions>().Bind(configuration.GetSection("PlayerPortfolio"))
                .Services
                .AddHostedService<CalculatePlayerPortfolioJob>();
        }

        if (enabled is null || enabled.Contains(nameof(CreatorValueMonitorJob)))
        {
            services
                .AddOptions<CreatorNetChangeOptions>().Bind(configuration.GetSection("CreatorValues"))
                .Services
                .AddHostedService<CreatorValueMonitorJob>();
        }

        if (enabled is null || enabled.Contains(nameof(StreamMonitorJob)))
        {
            services.AddHostedService<StreamMonitorJob>();
        }

        return services;
    }
}
