using TTX.Dto.Creators;

namespace TTX.Queries.Creators.FindCreator
{
    public class FindCreatorQuery : IQuery<CreatorDto?>
    {
        public required string Slug { get; init; }
        public required HistoryParams HistoryParams { get; init; }
    }
}