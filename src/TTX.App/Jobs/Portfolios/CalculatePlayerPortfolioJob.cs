using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TTX.App.Data;
using TTX.App.Events;
using TTX.App.Events.Players;
using TTX.App.Options;
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
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        ConfiguredCancelableAsyncEnumerable<Player> players = dbContext.Players
            .Include(p => p.Transactions.OrderBy(t => t.CreatedAt))
            .ThenInclude(t => t.Creator)
            .ToAsyncEnumerable()
            .WithCancellation(stoppingToken);

        await foreach (Player player in players)
        {
            PortfolioSnapshot snapshot = player.TakePortfolioSnapshot();
            await _events.Dispatch(UpdatePlayerPortfolioEvent.Create(snapshot));
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }
}
