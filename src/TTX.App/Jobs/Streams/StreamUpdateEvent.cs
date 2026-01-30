namespace TTX.App.Jobs.Streams;

public record StreamUpdateEvent
{
    public required int CreatorId { get; init; }
    public required bool IsLive { get; init; }
    public required DateTimeOffset At { get; init; }
}
