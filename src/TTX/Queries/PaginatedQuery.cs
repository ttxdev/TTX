namespace TTX.Queries;

public class PaginatedQuery<TResponse> : IQuery<Pagination<TResponse>>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 10;
    public Order[] Order { get; init; } = [];
    public Search? Search { get; init; } = null;
}

public readonly struct Search
{
    public required string By { get; init; }
    public required string Value { get; init; }
}

public struct Order
{
    public required string By { get; set; }
    public required OrderDirection Dir { get; set; }
}

public enum OrderDirection
{
    Ascending,
    Descending
}
