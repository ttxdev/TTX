namespace TTX.Domain.ValueObjects;

public abstract class ValueObject<T> : IEquatable<ValueObject<T>> where T : notnull
{
    public required T Value { get; init; }

    public override string ToString()
    {
        return Value.ToString()!;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            T other => Value.Equals(other),
            ValueObject<T> other => Value.Equals(other.Value),
            _ => false
        };
    }

    public bool Equals(ValueObject<T>? other)
    {
        throw new NotImplementedException();
    }

    public static bool operator ==(ValueObject<T>? left, ValueObject<T>? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Value.Equals(right.Value);
    }

    public static bool operator !=(ValueObject<T> left, ValueObject<T> right)
    {
        return !left.Value.Equals(right.Value);
    }

    public static implicit operator T(ValueObject<T> obj)
    {
        return obj.Value;
    }
}
