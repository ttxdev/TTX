using TTX.Domain.Exceptions;

namespace TTX.Domain.ValueObjects;

public class Quantity : ValueObject<int>
{
    public static Quantity Create(int value)
    {
        if (value < 0)
        {
            throw new InvalidValueObjectException<Quantity>("cannot be less than zero.");
        }

        return new Quantity { Value = value };
    }

    public static implicit operator Quantity(int value)
    {
        return Create(value);
    }
}
