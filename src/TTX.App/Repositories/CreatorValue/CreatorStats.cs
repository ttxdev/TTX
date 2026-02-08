using TTX.Domain.ValueObjects;

namespace TTX.App.Repositories.CreatorValue;

public record CreatorStats
{
    public double Positive { get; set; } = 0;
    public double Negative { get; set; } = 0;
    public int MessageCount { get; set; } = 0;
    public required Slug CreatorSlug { get; set; }
}
