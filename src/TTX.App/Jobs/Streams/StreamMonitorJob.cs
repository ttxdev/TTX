using TTX.App.Events;
using TTX.Domain.Models;
using TTX.App.Events.Creators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TTX.App.Repositories;

namespace TTX.App.Jobs.Streams;

public class StreamMonitorJob(
    ILogger<StreamMonitorJob> _logger,
    IServiceScopeFactory _scopeFactory,
    IEventDispatcher _events
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IStreamMonitorAdapter[] adapters = [];
        List<Creator> creators = [];
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        {
            ICreatorRepository repository = scope.ServiceProvider.GetRequiredService<ICreatorRepository>();
            adapters = [.. scope.ServiceProvider.GetServices<IStreamMonitorAdapter>()];
            // TODO optimize
            await foreach (Creator creator in repository.GetAll().WithCancellation(stoppingToken))
            {
                creators.Add(creator);
            }
        }

        foreach (IStreamMonitorAdapter adapter in adapters)
        {
            adapter.SetCreators(creators);
            adapter.StreamStatusUpdated += UpdateStreamStatus;
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Monitoring {creators}", string.Join(' ', creators.Select(c => c.Name)));
        }
        await Task.WhenAll(adapters.Select(a => a.Start()));
    }

    public async void UpdateStreamStatus(object? sender, StreamUpdateEvent @event)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        ICreatorRepository repository = scope.ServiceProvider.GetRequiredService<ICreatorRepository>();
        Creator? creator = await repository.Find(@event.CreatorId);
        if (creator is null)
        {
            if (sender is IStreamMonitorAdapter adapter)
            {
                adapter.RemoveCreator(@event.CreatorId);
            }

            return;
        }

        repository.Update(creator);
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

        await repository.SaveChanges();
        await _events.Dispatch(UpdateStreamStatusEvent.Create(creator));
    }
}
