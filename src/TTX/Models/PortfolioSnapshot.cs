using TTX.ValueObjects;

namespace TTX.Models
{
    public class PortfolioSnapshot
    {
        public required long Value { get; init; }
        public required ModelId PlayerId { get; init; }
        public DateTimeOffset Time { get; init; } = DateTimeOffset.UtcNow;
        public Player Player { get; init; } = null!;
    }
}