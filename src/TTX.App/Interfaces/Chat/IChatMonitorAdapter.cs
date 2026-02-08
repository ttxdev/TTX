namespace TTX.App.Interfaces.Chat;

public interface IChatMonitorAdapter
{
    event EventHandler<Message>? OnMessage;

    public Task Start(IEnumerable<string> channels, CancellationToken stoppingToken = default);
    public Task<bool> Add(string channel);
    public Task<bool> Remove(string channel);
}
