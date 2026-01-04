namespace TTX.App.Dto.Portfolio;

public readonly struct HistoryParams
{
    public required TimeStep Step { get; init; }
    public required DateTimeOffset After { get; init; }
}
