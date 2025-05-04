namespace TTX.Queries
{
    public readonly struct HistoryParams
    {
        public required TimeStep Step { get; init; }
        public required DateTimeOffset After { get; init; }
    }
}