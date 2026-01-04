using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TTX.App.Events;
using TTX.App.Events.Players;
using TTX.App.Options;
using TTX.App.Repositories;
using TTX.Domain.Models;

namespace TTX.App.Jobs.Portfolios;

public class CalculatePlayerPortfolioJob(
    IOptions<CalculatePlayerPortfolioOptions> _options,
    ILogger<CalculatePlayerPortfolioJob> _logger,
    IServiceProvider _services,
    IEventDispatcher _events
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CalculateAll(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error calculating player portfolios");
            }
            await Task.Delay(_options.Value.Delay, stoppingToken);
        }
    }

    public async Task CalculateAll(CancellationToken stoppingToken)
    {
        using AsyncServiceScope scope = _services.CreateAsyncScope();
        IPlayerRepository repository = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();
        ConfiguredCancelableAsyncEnumerable<Player> players = repository.SeekAll(stoppingToken);

        await foreach (Player player in players)
        {
            PortfolioSnapshot snapshot = player.TakePortfolioSnapshot();
            repository.Update(player);
            await _events.Dispatch(UpdatePlayerPortfolioEvent.Create(snapshot));
        }

        await repository.SaveChanges();
    }
}
