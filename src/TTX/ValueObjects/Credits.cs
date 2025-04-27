using TTX.Exceptions;

namespace TTX.ValueObjects;

public class Credits : ValueObject<long>
{
    public static Credits Create(long value)
    {
        if (value < 0)
            throw new InvalidValueObjectException(nameof(Credits), "cannot be negative.");

        return new Credits { Value = value };
    }

    public override string ToString() => Value.ToString();
    public override int GetHashCode() => Value.GetHashCode();
    public static implicit operator Credits(long value) => Create(value);
    public static implicit operator Credits(int value) => Create(value);
}
