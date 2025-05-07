using TTX.Dto.Players;

namespace TTX.Queries.Players.IndexPlayers
{
    public class IndexPlayersQuery : PaginatedQuery<PlayerOrderBy, PlayerDto>
    {
        public required HistoryParams HistoryParams { get; init; }
    }
}