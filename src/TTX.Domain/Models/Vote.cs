using TTX.Domain.ValueObjects;

namespace TTX.Domain.Models;

public class Vote
{
    public required Credits Value { get; init; }
    public DateTimeOffset Time { get; init; } = DateTime.UtcNow;
    public required ModelId CreatorId { get; init; }

    public virtual Creator Creator { get; init; } = null!;
}
