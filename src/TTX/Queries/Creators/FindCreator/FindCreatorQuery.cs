using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Queries.Creators.FindCreator
{
    public class FindCreatorQuery : IQuery<Creator?>
    {
        public required Slug Slug { get; init; }
        public required HistoryParams HistoryParams { get; init; }
    }
}