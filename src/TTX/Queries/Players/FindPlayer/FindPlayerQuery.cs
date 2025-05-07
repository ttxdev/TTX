using TTX.Dto.Players;

namespace TTX.Queries.Players.FindPlayer
{
    public readonly struct FindPlayerQuery : IQuery<PlayerDto?>
    {
        public required string Slug { get; init; }
        public required HistoryParams HistoryParams { get; init; }
    }
}