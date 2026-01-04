using TTX.Domain.Models;
using TTX.Domain.ValueObjects;

namespace TTX.App.Jobs.Streams;

public interface IStreamMonitorAdapter
{
    event EventHandler<StreamUpdateEvent>? StreamStatusUpdated;

    public Task Start(CancellationToken cancellationToken = default);
    public void SetCreators(IEnumerable<Creator> creators);
    public bool RemoveCreator(ModelId creatorId);
}
