using TTX.Models;

namespace TTX.Queries.Creators.IndexCreators;

public class IndexCreatorsQuery : PaginatedQuery<Creator>
{
    public required HistoryParams HistoryParams { get; init; }
}
