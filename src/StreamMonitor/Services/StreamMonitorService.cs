using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TTX.Core.Interfaces;
using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Interface.StreamMonitor.Services;

public class StreamMonitorService
{
  private readonly IServiceProvider serviceProvider;
  private readonly IStreamService streamService;
  private readonly ILogger logger;

  public StreamMonitorService(IServiceProvider serviceProvider, IStreamService streamService, ILogger logger)
  {
    this.serviceProvider = serviceProvider;
    this.streamService = streamService;
    this.logger = logger;
    this.streamService.OnStreamUpdate += async (_, update) => await OnStreamUpdate(update);
  }

  public async Task Start(CancellationToken token)
  {
    await streamService.Start(token);
    while (!token.IsCancellationRequested) { }
  }

  public async Task OnStreamUpdate(StreamStatusUpdate update)
  {
    logger.LogInformation("Stream update: {CreatorId} is {Status}", update.CreatorId, update.IsLive ? "online" : "offline");
    using var scope = serviceProvider.CreateAsyncScope();
    {
      var creatorRepository = scope.ServiceProvider.GetRequiredService<ICreatorRepository>();
      await creatorRepository.UpdateStreamInfo(update.CreatorId, update);
    }
  }

  public void AddCreator(Creator creator) => streamService.AddCreator(creator);
  public void RemoveCreator(Creator creator) => streamService.RemoveCreator(creator);
}
