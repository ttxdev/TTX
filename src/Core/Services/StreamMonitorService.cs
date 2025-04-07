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
        streamService.OnStreamUpdate += async (s, e) =>
        {
            var creator = await creatorRepository.Find(e.CreatorId);
            if (creator == null)
                return;

            creator.StreamStatus = e;
            creatorRepository.Update(creator);
            await creatorRepository.SaveChanges(token);
        };
        await streamService.Start(token);

        while (!token.IsCancellationRequested) { }
    }
}
