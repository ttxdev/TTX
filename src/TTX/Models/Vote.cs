using TTX.ValueObjects;

namespace TTX.Models
{
    public class Vote
    {
        public required Credits Value { get; init; }
        public required DateTimeOffset Time { get; init; }
        public required ModelId CreatorId { get; init; }
        public Creator Creator { get; init; } = null!;
    }
}