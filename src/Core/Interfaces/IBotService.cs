using TTX.Core.Models;

namespace TTX.Core.Interfaces;

public interface IBotService
{
    Task Start(CancellationToken token);
    event EventHandler<Message> OnMessage;
}
