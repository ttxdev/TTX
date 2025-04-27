namespace TTX.Queries;

public class Pagination<T>
{
    public required T[] Data { get; init; }
    public required int Total { get; init; }
}