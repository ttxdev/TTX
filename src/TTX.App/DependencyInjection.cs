using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TTX.App.Jobs.CreatorValues;
using TTX.App.Jobs.Portfolios;
using TTX.App.Jobs.Streams;
using TTX.App.Options;
using TTX.App.Services.Creators;
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
          // Services
          .AddScoped<CreatorService>()
          .AddScoped<TransactionService>()
          .AddScoped<PlayerService>();
    }

    public static IServiceCollection AddTtxJobs(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddOptions<CalculatePlayerPortfolioOptions>().Bind(configuration.GetSection("PlayerPortfolio"))
            .Services
            .AddHostedService<CalculatePlayerPortfolioJob>()
            .AddHostedService<CreatorValueMonitorJob>()
            .AddHostedService<StreamMonitorJob>();
    }
}
