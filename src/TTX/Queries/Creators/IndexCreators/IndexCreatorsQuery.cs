using TTX.Dto.Creators;

namespace TTX.Queries.Creators.IndexCreators
{
    public class IndexCreatorsQuery : PaginatedQuery<CreatorOrderBy, CreatorDto>
    {
        public required HistoryParams HistoryParams { get; init; }
    }
}