using Microsoft.Extensions.DependencyInjection;
using TTX.Domain.Models;
using TTX.App.Events;
using TTX.App.Events.Creators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using TTX.App.Repositories;

namespace TTX.App.Jobs.CreatorValues;

public class CreatorValueMonitorJob(
    IServiceProvider _services,
    IEventDispatcher _events,
    ILogger<CreatorValueMonitorJob> _logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<Creator> creators = [];
        List<Task> tasks = [];
        IChatMonitorAdapter[] chatMonitors;
        using (AsyncServiceScope scope = _services.CreateAsyncScope())
        {
            ICreatorRepository repository = scope.ServiceProvider.GetRequiredService<ICreatorRepository>();
            // TODO optimize
            await foreach (Creator creator in repository.GetAll().WithCancellation(stoppingToken))
            {
                creators.Add(creator);
            }
            chatMonitors = [.. scope.ServiceProvider.GetServices<IChatMonitorAdapter>()];
        }

        foreach (IChatMonitorAdapter chatMonitor in chatMonitors)
        {
            chatMonitor.OnMessage += OnMessageReceived;
            chatMonitor.SetCreators(creators);
            tasks.Add(chatMonitor.Start(stoppingToken));
        }

        if (chatMonitors.Length == 0)
        {
            _logger.LogWarning("No chat monitors configured");
        }
        else if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Watching {creatorNames}", string.Join(" ", creators.Select(c => c.Name)));
        }

        await Task.WhenAll(tasks);
    }

    public async void OnMessageReceived(object? _, MessageEvent e)
    {
        int value = GetValue(e.Content);
        if (value == 0)
        {
            return;
        }

        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        ICreatorRepository repository = scope.ServiceProvider.GetRequiredService<ICreatorRepository>();
        Creator? creator = await repository.Find(e.CreatorId);
        if (creator is null)
        {
            return;
        }

        repository.Update(creator);
        int netChange = GetValue(e.Content);
        Vote vote = creator.ApplyNetChange(netChange);
        await repository.StoreVote(vote);
        await repository.SaveChanges();
        await _events.Dispatch(UpdateCreatorValueEvent.Create(vote));

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("{creatorSlug} {diff} {value}", creator.Slug, value > 0 ? "gained" : "lost", value);
        }
    }

    private static int GetValue(string content)
    {
        if (content.Contains("+2"))
        {
            return 2;
        }

        if (content.Contains("-2"))
        {
            return -2;
        }

        return 0;
    }
}
