using TTX.Core.Interfaces;

namespace TTX.Core.Services;

public interface IValueMonitorService
{
    Task Start(CancellationToken token);
}

public class ValueMonitorService(ICreatorValueService valEvalService, IBotService botService, int bufferTime) : IValueMonitorService
{
    public async Task Start(CancellationToken token)
    {
        botService.OnMessage += (s, e) => valEvalService.Process(e.Creator, e.Content);
        await botService.Start(token);

        while (!token.IsCancellationRequested)
        {
            await valEvalService.Digest();
            await Task.Delay(bufferTime, token);
        }
    }
}