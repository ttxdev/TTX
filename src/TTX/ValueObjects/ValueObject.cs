namespace TTX.ValueObjects;

public abstract class ValueObject<T> where T : notnull
{
    public required T Value { get; init; }
    public override string ToString() => Value.ToString()!;
    public override int GetHashCode() => Value.GetHashCode();

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            T other => Value.Equals(other),
            ValueObject<T> other => Value.Equals(other.Value),
            _ => false
        };
    }

    public static bool operator ==(ValueObject<T> left, ValueObject<T> right)
    {
        return left.Value.Equals(right.Value);
    }

    public static bool operator !=(ValueObject<T> left, ValueObject<T> right)
    {
        return !left.Value.Equals(right.Value);
    }

    public static implicit operator T(ValueObject<T> obj) => obj.Value;
}
