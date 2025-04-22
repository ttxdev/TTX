using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Commands.Creators.UpdateStreamStatus;

public readonly struct UpdateStreamStatusCommand : ICommand<StreamStatus>
{
    public required Slug CreatorSlug { get; init; }
    public required bool IsLive { get; init; }
    public required DateTimeOffset At { get; init; }
}
