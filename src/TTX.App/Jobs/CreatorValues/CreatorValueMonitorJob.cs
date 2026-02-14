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
using System.Threading.Channels;

namespace TTX.App.Jobs.CreatorValues;

public class CreatorValueMonitorJob(
    IServiceScopeFactory _scopes,
    IEventDispatcher _dispatcher,
    IEventReceiver _events,
    ILogger<CreatorValueMonitorJob> _logger,
    IOptions<CreatorValuesJobOptions> _options
) : BackgroundService
{
    private readonly Channel<Message> _messageChannel = Channel.CreateUnbounded<Message>();
    private IChatMonitorAdapter[] _chatMonitors = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        AsyncServiceScope scope = _scopes.CreateAsyncScope();
        ChatMonitorFactory chatFactory = scope.ServiceProvider.GetRequiredService<ChatMonitorFactory>();
        List<Task> _tasks = [];
        _chatMonitors = [.. chatFactory.CreateAll()];
        if (_chatMonitors.Length == 0)
        {
            _logger.LogWarning("No chat monitors configured.");
            return;
        }

        foreach (IChatMonitorAdapter monitor in _chatMonitors)
        {
            monitor.OnMessage += (_, msg) => _messageChannel.Writer.TryWrite(msg);
            await monitor.Start([], stoppingToken);
        }

        _tasks.Add(ProcessMessagesAsync(stoppingToken));
        _tasks.Add(_events.OnEventReceived<UpdateStreamStatusEvent>(async (@event, ct) =>
        {
            using AsyncServiceScope dbScope = _scopes.CreateAsyncScope();
            ApplicationDbContext dbContext = dbScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Creator? creator = await dbContext.Creators.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == @event.CreatorId, ct);

            if (creator is null) return;

            IChatMonitorAdapter? monitor = _chatMonitors.FirstOrDefault();

            if (monitor != null)
            {
                if (creator.StreamStatus.IsLive)
                {
                    await monitor.Add(creator.Slug.Value.ToLower());
                }
                else
                {
                    await monitor.Remove(creator.Slug.Value.ToLower());
                }
            }
        }, stoppingToken));
        _tasks.Add(Task.Run(async () =>
            {
                using PeriodicTimer timer = new(_options.Value.Delay);
                try
                {
                    while (await timer.WaitForNextTickAsync(stoppingToken))
                    {
                        try
                        {
                            await DigestAll();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "An error occurred during the periodic digest.");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Digest loop shutting down.");
                }
            }, stoppingToken));

        await Task.WhenAny(_tasks);
        await scope.DisposeAsync();
    }

    private async Task ProcessMessagesAsync(CancellationToken ct)
    {
        await foreach (Message message in _messageChannel.Reader.ReadAllAsync(ct))
        {
            try
            {
                await ParseMessage(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing message");
            }
        }
    }

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

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Channel {Slug} ({Compound:F2}): {Message} ", m.Slug, value, m.Content);
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

        Console.WriteLine("Digesting");
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
