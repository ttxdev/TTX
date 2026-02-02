using Microsoft.Extensions.DependencyInjection;
using TTX.Domain.Models;
using TTX.App.Events;
using TTX.App.Events.Creators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using TTX.App.Data;
using TTX.App.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TTX.App.Jobs.CreatorValues;

public class CreatorValueMonitorJob(
    IServiceProvider _services,
    IEventDispatcher _events,
    ILogger<CreatorValueMonitorJob> _logger
) : IHostedService
{
    private readonly Queue<NetChangeEvent> _queue = new();
    private IChatMonitorAdapter[] _chatMonitors = [];

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        List<Creator> creators = [];
        List<Task> tasks = [];
        using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // TODO optimize
            creators = await dbContext.Creators.ToListAsync(stoppingToken);
            _chatMonitors = [.. scope.ServiceProvider.GetServices<IChatMonitorAdapter>().Select(chatMonitor =>
                    {
                        chatMonitor.OnNetChange += OnNetChangeReceived;
                        chatMonitor.SetCreators(creators);
                        tasks.Add(chatMonitor.Start(stoppingToken));
                        return chatMonitor;
                    })];
        }

        if (_chatMonitors.Length == 0)
        {
            _logger.LogWarning("No chat monitors configured");
        }
        else if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Watching {creatorNames}", string.Join(" ", creators.Select(c => c.Name)));
        }

        await Task.WhenAll(tasks);
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out NetChangeEvent? e))
            {
                await Digest(e!);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.WhenAll(_chatMonitors.Select(c => c.Stop(cancellationToken)));
    }

    private void OnNetChangeReceived(object? _, NetChangeEvent e)
    {
        _queue.Enqueue(e);
    }

    private async Task Digest(NetChangeEvent e)
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        PortfolioRepository portfolioRepository = scope.ServiceProvider.GetRequiredService<PortfolioRepository>();
        Creator? creator = await dbContext.Creators.FirstOrDefaultAsync(c => c.Id == e.CreatorId);
        if (creator is null)
        {
            return;
        }

        Vote vote = creator.ApplyNetChange(e.NetChange);
        await dbContext.SaveChangesAsync();
        await portfolioRepository.StoreVote(vote);
        await _events.Dispatch(UpdateCreatorValueEvent.Create(vote));

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("{creatorSlug} {diff} {value}", creator.Slug, e.NetChange > 0 ? "gained" : "lost", e.NetChange);
        }
    }
}
