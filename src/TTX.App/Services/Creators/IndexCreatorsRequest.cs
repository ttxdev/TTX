using TTX.App.Dto.Pagination;
using TTX.App.Dto.Portfolio;

namespace TTX.App.Services.Creators;

public record IndexCreatorsRequest : PaginatedRequest<CreatorOrderBy>
{
    public int? MinValue { get; init; } = null;
    public required HistoryParams HistoryParams { get; init; }
}
