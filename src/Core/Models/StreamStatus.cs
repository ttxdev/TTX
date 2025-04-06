namespace TTX.Core.Models;

public class StreamStatus
{
    public bool IsLive { get; set; } = false;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}