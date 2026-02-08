using TTX.App.Events;
using TTX.Domain.Models;
using TTX.App.Events.Creators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TTX.App.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace TTX.App.Jobs.Streams;

public class StreamMonitorJob(
    ILogger<StreamMonitorJob> _logger,
    IServiceScopeFactory _scopeFactory,
    IEventDispatcher _events
) : BackgroundService
{
    private IStreamMonitorAdapter[] _adapters = [];
    private readonly ConcurrentQueue<StreamUpdateEvent> _queue = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Creator[] creators = [];
        List<Task> tasks = [];
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        {
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Creators.ExecuteUpdateAsync(
                s =>
                    s.SetProperty(c => c.StreamStatus.IsLive, false)
                    .SetProperty(c => c.StreamStatus.EndedAt, DateTime.UtcNow),
                stoppingToken
            );
            // TODO optimize
            creators = await dbContext.Creators.AsNoTracking().ToArrayAsync(cancellationToken: stoppingToken);
        }

        _adapters = [.. scope.ServiceProvider.GetServices<IStreamMonitorAdapter>().Select(adapter =>
            {
                adapter.SetCreators(creators);
                adapter.StreamStatusUpdated += UpdateStreamStatus;
                tasks.Add(adapter.Start(stoppingToken));
                return adapter;
            })];

        if (_adapters.Length == 0)
        {
            _logger.LogWarning("No stream monitor adapters found");
            return;
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Monitoring {creators}", string.Join(' ', creators.Select(c => c.Name)));
        }

        tasks.Add(Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await DigestAll();
                    await Task.Delay(300, stoppingToken);
                }
            }, stoppingToken));

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get streams");
        }
    }

    public void UpdateStreamStatus(object? sender, StreamUpdateEvent @event)
    {
        _queue.Enqueue(@event);
    }

    private async Task DigestAll()
    {
        while (!_queue.IsEmpty)
        {
            if (_queue.TryDequeue(out StreamUpdateEvent? @event) || @event is null)
            {
                continue;
            }

            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Creator? creator = await dbContext.Creators.FirstOrDefaultAsync(c => c.Id == @event.CreatorId);
            if (creator is null)
            {
                foreach (IStreamMonitorAdapter adapter in _adapters)
                {
                    adapter.RemoveCreator(@event.CreatorId);
                }

                return;
            }

            if (@event.IsLive)
            {
                creator.StreamStatus.Started(@event.At);
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("{creator} is now live on {platform}", creator.Name, creator.Platform);
                }
            }
            else
            {
                creator.StreamStatus.Ended(@event.At);
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("{creator} ended stream on {platform}", creator.Name, creator.Platform);
                }
            }

            await dbContext.SaveChangesAsync();
            await _events.Dispatch(UpdateStreamStatusEvent.Create(creator));
        }
    }
}
