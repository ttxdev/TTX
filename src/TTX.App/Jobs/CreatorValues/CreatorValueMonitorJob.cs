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
using TTX.App.Factories;
using TTX.App.Interfaces.CreatorValue;
using TTX.App.Repositories.CreatorValue;
using TTX.App.Interfaces.Chat;

namespace TTX.App.Jobs.CreatorValues;

public class CreatorValueMonitorJob(
    IServiceScopeFactory _scopes,
    ChatMonitorFactory _chatMonitorFactory,
    IEventDispatcher _dispatcher,
    IEventReceiver _events,
    ILogger<CreatorValueMonitorJob> _logger,
    IOptions<CreatorValuesJobOptions> _options
) : BackgroundService
{
    private readonly ConcurrentQueue<Message> _queue = [];
    private readonly List<Task> _tasks = [];
    private IChatMonitorAdapter[] _chatMonitors = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (AsyncServiceScope scope = _scopes.CreateAsyncScope())
        {
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _chatMonitors = [.. _chatMonitorFactory.CreateAll().Select(chatMonitor =>
                    {
                        chatMonitor.OnMessage += OnMessage;
                        return chatMonitor;
                    })];
        }

        if (_chatMonitors.Length == 0)
        {
            _logger.LogWarning("No chat monitors configured, stopping.");
            return;
        }

        _tasks.Add(_events.OnEventReceived<UpdateStreamStatusEvent>(async (@event, ct) =>
        {
            await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Creator? creator = await dbContext.Creators.AsNoTracking().FirstOrDefaultAsync(c => c.Id == @event.CreatorId, ct);
            if (creator is null)
            {
                return;
            }

            IChatMonitorAdapter chatMonitor = scope.ServiceProvider.GetRequiredKeyedService<IChatMonitorAdapter>(creator.Platform);
            if (creator.StreamStatus.IsLive)
            {
                await chatMonitor.Add(creator.Slug);
            }
            else
            {
                await chatMonitor.Remove(creator.Slug);
            }
        }, stoppingToken));

        _tasks.Add(Task.Run(async () =>
        {
            using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(300));
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    if (_queue.TryDequeue(out Message? m))
                    {
                        await ParseMessage(m);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }, stoppingToken));

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
            }
        }, stoppingToken));

        await Task.WhenAll(_tasks);
    }

    public void OnMessage(object? _, Message m) => _queue.Enqueue(m);

    private async Task ParseMessage(Message m)
    {
        await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
        IMessageAnalyzer _analyzer = scope.ServiceProvider.GetRequiredService<IMessageAnalyzer>();
        ICreatorStatsRepository statsRepository = scope.ServiceProvider.GetRequiredService<ICreatorStatsRepository>();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        bool? isLive = await dbContext.Creators
            .Where(c => c.Slug == m.Slug)
            .Select(c => c.StreamStatus.IsLive)
            .FirstOrDefaultAsync();

        if (isLive is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("Channel {Slug} not found in database. Removing.", m.Slug);
            }

            await Task.WhenAll(_chatMonitors.Select(c => c.Remove(m.Slug)));
            return;
        }

        if (!isLive.Value)
        {
            return;
        }

        double value = await _analyzer.Analyze(m.Content);
        CreatorStats stats = await statsRepository.GetByCreator(m.Slug);

        stats.MessageCount++;
        if (value > 0) stats.Positive++;
        else if (value < 0) stats.Negative++;

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Channel {Slug} ({Compound:F2}): {Message} ", m.Slug, value, m.Content);
        }
        await statsRepository.SetByCreator(m.Slug, stats);
    }

    private async Task DigestAll()
    {
        await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        ICreatorStatsRepository statsRepository = scope.ServiceProvider.GetRequiredService<ICreatorStatsRepository>();
        PortfolioRepository portfolioRepository = scope.ServiceProvider.GetRequiredService<PortfolioRepository>();
        IStatsProcessor statsProcessor = scope.ServiceProvider.GetRequiredService<IStatsProcessor>();
        Creator[] creators = await dbContext.Creators.ToArrayAsync();
        CreatorStats[] allStats = await statsRepository.GetAll(true);

        foreach (Creator creator in creators)
        {
            CreatorStats? stats = allStats.FirstOrDefault(c => c.CreatorSlug == creator.Slug);
            double netChange = await statsProcessor.Process(creator, stats);
            if (netChange == 0.0)
            {
                continue;
            }

            Vote vote = creator.ApplyNetChange(netChange);
            await dbContext.SaveChangesAsync();
            await portfolioRepository.StoreVote(vote);
            await _dispatcher.Dispatch(UpdateCreatorValueEvent.Create(vote));

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("{creatorSlug} {diff} {value}", creator.Slug, netChange > 0 ? "gained" : "lost", netChange);
            }
        }
    }
}
