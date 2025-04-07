using TTX.Core.Interfaces;
using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Core.Services;

public interface IValueMonitorService
{
    Task Start(CancellationToken token);
    void AddCreator(Creator creator);
    void RemoveCreator(Creator creator);
}

public class ValueMonitorService(
    ICreatorRepository creatorRepository,
    ICreatorValueService valEvalService,
    IBotService botService,
    int bufferTime
) : IValueMonitorService
{
    public async Task Start(CancellationToken token)
    {
        botService.OnMessage += (s, e) => valEvalService.Process(e.Creator, e.Content);

        var creators = await creatorRepository.GetAll();
        foreach (var creator in creators) botService.AddCreator(creator);

        await botService.Start(token);
        while (!token.IsCancellationRequested)
        {
            await valEvalService.Digest();
            await Task.Delay(bufferTime, token);
        }
    }

    public void AddCreator(Creator creator) => botService.AddCreator(creator);
    public void RemoveCreator(Creator creator) => botService.RemoveCreator(creator);
}