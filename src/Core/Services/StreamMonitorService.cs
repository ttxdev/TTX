using TTX.Core.Interfaces;
using TTX.Core.Repositories;

namespace TTX.Core.Services;

public interface IStreamMonitorService
{
    Task Start(CancellationToken token);
}

public class StreamMonitorService(IStreamService streamService, ICreatorRepository creatorRepository) : IStreamMonitorService
{
    public async Task Start(CancellationToken token)
    {
        streamService.OnStreamUpdate += async (s, e) => await creatorRepository.UpdateStreamInfo(e.CreatorId, e);
        await streamService.Start(token);

        while (!token.IsCancellationRequested) { }
    }
}
