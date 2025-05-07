using TTX.Dto;

namespace TTX.Queries
{
    public class PaginatedQuery<TOrder, TResponse> : IQuery<PaginationDto<TResponse>>
    {
        public int Page { get; init; } = 1;
        public int Limit { get; init; } = 10;
        public string? Search { get; init; } = null;
        public Order<TOrder>? Order { get; init; }
    }
}