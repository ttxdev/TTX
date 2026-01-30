// using Microsoft.Extensions.DependencyInjection;
// using TTX.App.Data;
// using TTX.App.Jobs.Streams;
// using TTX.Domain.Models;
// using TTX.Tests.App.Factories;
// using TTX.Tests.App.Infrastructure.Streams;

namespace TTX.Tests.App.Jobs.Streams;

[TestClass]
public class StreamMonitorJobTests : ServiceTests
{
    [TestMethod]
    public async Task TestJob()
    {

        Assert.Inconclusive("todo");
        /*
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        CreatorFactory creatorFactory = scope.ServiceProvider.GetRequiredService<CreatorFactory>();
        TestStreamMonitorAdapter adapter = (scope.ServiceProvider.GetRequiredService<IStreamMonitorAdapter>() as TestStreamMonitorAdapter)!;
        StreamMonitorJob job = scope.ServiceProvider.GetRequiredService<StreamMonitorJob>();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Creator creator = creatorFactory.Create();
        dbContext.Creators.Add(creator);
        TimeSpan delay = TimeSpan.FromDays(10);
        DateTime startedAt = DateTime.UtcNow;
        DateTime endedAt = startedAt.Add(delay);
        await dbContext.SaveChangesAsync(TestContext.CancellationToken);

        CancellationTokenSource csr = new();
        try
        {
            await Task.WhenAll(
                job.StartAsync(csr.Token),
                Task.Run(async () =>
                {
                    adapter.Dispatch(new StreamUpdateEvent
                    {
                        CreatorId = creator.Id,
                        IsLive = true,
                        At = startedAt
                    });
                    adapter.Dispatch(new StreamUpdateEvent
                    {
                        CreatorId = creator.Id,
                        IsLive = false,
                        At = endedAt
                    });
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    await csr.CancelAsync();
                }, TestContext.CancellationToken)
            );
        }
        catch (OperationCanceledException)
        {

        }
        Assert.IsFalse(creator.StreamStatus.IsLive);
        Assert.AreEqual(creator.StreamStatus.StartedAt, startedAt);
        Assert.AreEqual(creator.StreamStatus.EndedAt, endedAt);
        */
    }
}
