using TTX.App.Jobs.Streams;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;

namespace TTX.Tests.App.Infrastructure.Streams;

public class TestStreamMonitorAdapter : IStreamMonitorAdapter
{
    public event EventHandler<StreamUpdateEvent>? StreamStatusUpdated;

    public void SetCreators(IEnumerable<Creator> creators)
    {
    }

    public async Task Start(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }

    public void Dispatch(StreamUpdateEvent streamUpdateEvent)
    {
        StreamStatusUpdated?.Invoke(this, streamUpdateEvent);
    }

    public bool RemoveCreator(ModelId creatorId)
    {
        throw new NotImplementedException();
    }
}
