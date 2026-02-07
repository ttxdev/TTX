using Microsoft.Extensions.DependencyInjection;
using TTX.Domain.Models;
using TTX.App.Events;
using TTX.App.Events.Creators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using TTX.App.Data;
using TTX.App.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TTX.App.Options;
using System.Collections.Concurrent;

namespace TTX.App.Jobs.CreatorValues;

public class CreatorValueMonitorJob(
    IServiceProvider _services,
    IEventDispatcher _events,
    ILogger<CreatorValueMonitorJob> _logger,
    IOptions<CreatorNetChangeOptions> _options
) : BackgroundService
{
    private readonly ConcurrentQueue<NetChangeEvent> _queue = new();
    private readonly List<Task> _tasks = [];
    private IChatMonitorAdapter[] _chatMonitors = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<Creator> creators = [];
        using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // TODO optimize
            creators = await dbContext.Creators.ToListAsync(stoppingToken);
            _chatMonitors = [.. scope.ServiceProvider.GetServices<IChatMonitorAdapter>().Select(chatMonitor =>
                    {
                        chatMonitor.OnNetChange += OnNetChangeReceived;
                        chatMonitor.SetCreators(creators);
                        _tasks.Add(chatMonitor.Start(stoppingToken));
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

        _tasks.Add(Task.Run(async () =>
        {
            using PeriodicTimer timer = new(_options.Value.Delay);
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await DigestAll();
                }
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown expected
            }
        }, stoppingToken));

        await Task.WhenAll(_tasks);
    }

    private void OnNetChangeReceived(object? _, NetChangeEvent e)
    {
        _queue.Enqueue(e);
    }

    private async Task DigestAll()
    {
        if (_queue.IsEmpty) return;

        List<NetChangeEvent> batch = [];
        while (_queue.TryDequeue(out var e))
        {
            batch.Add(e);
        }

        var aggregatedChanges = batch
            .GroupBy(e => e.CreatorId)
            .Select(g => new
            {
                CreatorId = g.Key,
                TotalChange = g.Sum(e => e.NetChange)
            })
            .Where(x => x.TotalChange != 0)
            .ToList();

        await using AsyncServiceScope scope = _services.CreateAsyncScope();

        foreach (var update in aggregatedChanges)
        {
            await Digest(new NetChangeEvent(update.CreatorId, update.TotalChange));
        }

        if (_logger.IsEnabled(LogLevel.Debug) && batch.Count > 0)
        {
            _logger.LogDebug("Ticker processed {EventCount} events into {UpdateCount} updates.", batch.Count, aggregatedChanges.Count);
        }
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
