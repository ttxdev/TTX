namespace TTX.Queries.Creators;

public readonly struct HistoryParams
{
    public required TimeStep Step { get; init; }
    public required DateTimeOffset After { get; init; }
}
