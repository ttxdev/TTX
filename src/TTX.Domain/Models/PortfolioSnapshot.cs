using TTX.Domain.ValueObjects;

namespace TTX.Domain.Models;

public class PortfolioSnapshot
{
    public required long Value { get; init; }
    public required ModelId PlayerId { get; init; }
    public DateTimeOffset Time { get; init; } = DateTime.UtcNow;

    public virtual Player Player { get; init; } = null!;
}
