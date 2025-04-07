using TTX.Core.Interfaces;
using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Core.Services;

public interface IStreamMonitorService
{
    Task Start(CancellationToken token);
    void AddCreator(Creator creator);
    void RemoveCreator(Creator creator);
}

public class StreamMonitorService : IStreamMonitorService
{
    private readonly IStreamService streamService;
    private readonly ICreatorRepository creatorRepository;
    private readonly Queue<StreamStatusUpdate> streamStatusQueue = new();
    public StreamMonitorService(IStreamService streamService, ICreatorRepository creatorRepository)
    {
        this.streamService = streamService;
        this.creatorRepository = creatorRepository;
        streamService.OnStreamUpdate += (s, e) => streamStatusQueue.Enqueue(e);
    }

    public void AddCreator(Creator creator) => streamService.AddCreator(creator);
    public void RemoveCreator(Creator creator) => streamService.RemoveCreator(creator);
    public async Task Start(CancellationToken token)
    {
        await streamService.Start(token);
        while (!token.IsCancellationRequested) {
            if (streamStatusQueue.TryDequeue(out var statusUpdate))
                await creatorRepository.UpdateStreamInfo(statusUpdate.CreatorId, statusUpdate);
        }
    }
}
