using TTX.Domain.Models;

namespace TTX.App.Jobs.CreatorValues;

public interface IChatMonitorAdapter
{
    event EventHandler<MessageEvent>? OnMessage;

    public Task Start(CancellationToken cancellationToken = default);
    public void SetCreators(IEnumerable<Creator> creators);
}
