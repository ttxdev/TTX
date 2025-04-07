using TTX.Core.Models;

namespace TTX.Core.Interfaces;

public class StreamStatusUpdate : StreamStatus
{
  public required int CreatorId { get; set; }
}

public interface IStreamService
{
    Task Start(CancellationToken token);
    void AddCreator(Creator creator);
    void RemoveCreator(Creator creator);
    event EventHandler<StreamStatusUpdate> OnStreamUpdate;
}
