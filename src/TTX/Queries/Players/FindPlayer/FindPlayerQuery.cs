using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Queries.Players.FindPlayer
{
    public readonly struct FindPlayerQuery : IQuery<Player?>
    {
        public required Slug Slug { get; init; }
        public required HistoryParams HistoryParams { get; init; }
    }
}