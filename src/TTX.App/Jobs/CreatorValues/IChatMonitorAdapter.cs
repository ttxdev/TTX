using TTX.Domain.Models;

namespace TTX.App.Jobs.CreatorValues;

public interface IChatMonitorAdapter
{
    event EventHandler<NetChangeEvent>? OnNetChange;

    public Task Start(CancellationToken stoppingToken = default);
    public void SetCreators(IEnumerable<Creator> creators);
}
