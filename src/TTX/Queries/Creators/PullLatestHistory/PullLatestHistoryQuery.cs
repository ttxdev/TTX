using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Queries.Creators.PullLatestHistory;

public readonly struct PullLatestHistoryQuery : IQuery<Vote[]>
{
    public required Slug CreatorSlug { get; init; }
    public required TimeStep Step { get; init; }
    public required DateTimeOffset After { get; init; }
}
