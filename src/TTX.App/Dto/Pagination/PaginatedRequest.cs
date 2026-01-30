namespace TTX.App.Dto.Pagination;

public record PaginatedRequest<TOrder>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 10;
    public string? Search { get; init; } = null;
    public Order<TOrder>? Order { get; init; } = null;
}
