using TTX.App.Dto.Pagination;
using TTX.App.Dto.Portfolio;

namespace TTX.App.Services.Players;

public sealed record IndexPlayersRequest : PaginatedRequest<PlayerOrderBy>
{
    public required HistoryParams HistoryParams { get; init; }
}
