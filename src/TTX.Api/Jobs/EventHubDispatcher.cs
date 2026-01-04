using Microsoft.AspNetCore.SignalR;
using TTX.Api.Hubs;
using TTX.App.Events;
using TTX.App.Events.Creators;
using TTX.App.Events.Players;
using TTX.App.Events.Transactions;

namespace TTX.Api.Jobs;

public class EventHubDispatcher(IEventReceiver _handler, IHubContext<EventHub> _hubContext) : IHostedService
{
    CancellationTokenSource? _cts = null;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = new CancellationTokenSource();
        return Task.WhenAll(
            Register<CreateTransactionEvent>(_cts.Token),
            Register<UpdateCreatorValueEvent>(_cts.Token),
            Register<UpdatePlayerPortfolioEvent>(_cts.Token),
            Register<UpdateStreamStatusEvent>(_cts.Token)
        );
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts is null)
        {
            return Task.CompletedTask;
        }

        return _cts.CancelAsync();
    }

    private Task Register<TEvent>(CancellationToken ct) where TEvent : IEvent
    {
        return _handler.OnEventReceived<TEvent>(Dispatch, ct);
    }

    private async void Dispatch<T>(T @event, CancellationToken cancellationToken = default) where T : IEvent
    {
        await _hubContext.Clients.All.SendAsync(typeof(T).Name, @event, cancellationToken);
    }
}
