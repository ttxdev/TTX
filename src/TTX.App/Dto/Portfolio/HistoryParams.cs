namespace TTX.App.Dto.Portfolio;

public readonly struct HistoryParams
{
    public required TimeStep Step { get; init; }
    public required TimeSpan Before { get; init; }
}
