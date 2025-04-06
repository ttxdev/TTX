using TTX.Core.Models;

namespace TTX.Core.Interfaces;

public class Message
{
    public required Creator Creator { get; set; }
    public required string Content { get; set; }
}

public interface IBotService
{
    Task Start(CancellationToken token);
    void AddCreator(Creator creator);
    void RemoveCreator(Creator creator);
    event EventHandler<Message> OnMessage;
}
