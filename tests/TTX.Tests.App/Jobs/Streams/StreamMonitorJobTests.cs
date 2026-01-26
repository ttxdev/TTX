using Microsoft.Extensions.DependencyInjection;
using TTX.App.Jobs.Streams;
using TTX.Tests.App.Infrastructure.Streams;

namespace TTX.Tests.App.Jobs.Streams;

[TestClass]
public class StreamMonitorJobTests : ServiceTests
{
    public virtual TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task TestJob()
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        TestStreamMonitorAdapter adapter = (_services.GetRequiredService<IStreamMonitorAdapter>() as TestStreamMonitorAdapter)!;
        StreamMonitorJob job = scope.ServiceProvider.GetRequiredService<StreamMonitorJob>();
        CancellationTokenSource csr = new();
        try
        {
            await Task.WhenAll(
                job.StartAsync(csr.Token),
                Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), TestContext.CancellationToken);
                    adapter.Dispatch(new StreamUpdateEvent()
                    {
                        CreatorId = 0,
                        IsLive = false,
                        At = DateTime.UtcNow
                    });
                    await Task.Delay(TimeSpan.FromSeconds(1), TestContext.CancellationToken);
                    await csr.CancelAsync();
                }, TestContext.CancellationToken)
            );
        }
        catch (OperationCanceledException)
        {

        }
    }
}
