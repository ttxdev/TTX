using TTX.Domain.Exceptions;

namespace TTX.Domain.ValueObjects;

public class Credits : ValueObject<double>
{
    public static bool TryValidate(double value, out string reason)
    {
        reason = string.Empty;
        if (value < 0)
        {
            reason = "cannot be negative.";
            return false;
        }

        return true;
    }

    public static Credits Create(double value)
    {
        if (!TryValidate(value, out var reason))
        {
            throw new InvalidValueObjectException<Credits>(reason);
        }

        return new Credits { Value = value };
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static implicit operator double(Credits credits)
    {
        return credits.Value;
    }

    public static implicit operator Credits(double value)
    {
        return Create(value);
    }

    public static implicit operator Credits(long value)
    {
        return Create(value);
    }

    public static implicit operator Credits(int value)
    {
        return Create(value);
    }
}
