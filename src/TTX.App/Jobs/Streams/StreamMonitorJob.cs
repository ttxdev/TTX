using TTX.App.Events;
using TTX.Domain.Models;
using TTX.App.Events.Creators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TTX.App.Data;
using Microsoft.EntityFrameworkCore;

namespace TTX.App.Jobs.Streams;

public class StreamMonitorJob(
    ILogger<StreamMonitorJob> _logger,
    IServiceScopeFactory _scopeFactory,
    IEventDispatcher _events
) : BackgroundService
{
    private IStreamMonitorAdapter[] _adapters = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Creator[] creators = [];
        List<Task> tasks = [];
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        {
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // TODO optimize
            creators = await dbContext.Creators.ToArrayAsync(cancellationToken: stoppingToken);
            _adapters = [.. scope.ServiceProvider.GetServices<IStreamMonitorAdapter>().Select(adapter =>
                {
                    adapter.SetCreators(creators);
                    adapter.StreamStatusUpdated += UpdateStreamStatus;
                    tasks.Add(adapter.Start(stoppingToken));
                    return adapter;
                })];
        }

        if (_adapters.Length > 0 && _logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Monitoring {creators}", string.Join(' ', creators.Select(c => c.Name)));
        }
        else if (_adapters.Length == 0)
        {
            _logger.LogWarning("No stream monitor adapters found");
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get streams");
        }
    }

    public async void UpdateStreamStatus(object? sender, StreamUpdateEvent @event)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Creator? creator = await dbContext.Creators.FirstOrDefaultAsync(c => c.Id == @event.CreatorId);
        if (creator is null)
        {
            if (sender is IStreamMonitorAdapter adapter)
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
