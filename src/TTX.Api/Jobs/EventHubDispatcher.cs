using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using TTX.Api.Hubs;
using TTX.App.Events;
using TTX.App.Events.Creators;
using TTX.App.Events.Players;
using TTX.App.Events.Transactions;

namespace TTX.Api.Jobs;

public class EventHubDispatcher(IEventReceiver _handler, IHubContext<EventHub> _hubContext) : IHostedService
{
    private CancellationTokenSource? _cts = null;
    private List<Task> _tasks = [];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _tasks.AddRange(
            Register<CreateTransactionEvent>(_cts.Token),
            Register<UpdateCreatorValueEvent>(_cts.Token),
            Register<UpdatePlayerPortfolioEvent>(_cts.Token),
            Register<UpdateStreamStatusEvent>(_cts.Token)
        );
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts is null)
        {
            return;
        }

        await _cts.CancelAsync();
        Task timeout = Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
        Task allFinished = Task.WhenAll(_tasks);

        await Task.WhenAny(allFinished, timeout);
    }

    private Task Register<T>(CancellationToken ct) where T : BaseEvent
    {
        return _handler.OnEventReceived<T>(Dispatch, ct);
    }

    private Task Dispatch<T>(T @event, CancellationToken cancellationToken = default) where T : BaseEvent
    {
        return _hubContext.Clients.All.SendAsync(@event.Type, @event, cancellationToken);
    }
}
