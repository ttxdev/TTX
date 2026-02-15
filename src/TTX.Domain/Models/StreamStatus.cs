namespace TTX.Domain.Models;

public class StreamStatus
{
    public bool IsLive { get; private set; } = false;
    public DateTimeOffset StartedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset EndedAt { get; private set; } = DateTimeOffset.UtcNow;

    public void Started(DateTimeOffset at)
    {
        StartedAt = at;
        IsLive = true;
    }

    public void Ended(DateTimeOffset at)
    {
        EndedAt = at;
        IsLive = false;
    }
}
