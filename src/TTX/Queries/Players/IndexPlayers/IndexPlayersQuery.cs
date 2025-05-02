using TTX.Models;

namespace TTX.Queries.Players.IndexPlayers
{
    public class IndexPlayersQuery : PaginatedQuery<PlayerOrderBy, Player>
    {
        public required HistoryParams HistoryParams { get; init; }
    }
}