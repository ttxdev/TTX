using TTX.Domain.Exceptions;

namespace TTX.Domain.ValueObjects;

public class Result<T>
{
    public T? Value { get; init; }
    public TtxException? Error { get; init; }
    public bool IsSuccessful => Error is null;

    public static Result<T> Ok(T value) => new() { Value = value };
    public static Result<T> Err(TtxException exception) => new() { Error = exception };
}
