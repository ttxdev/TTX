using TTX.Exceptions;

namespace TTX.ValueObjects
{
    public class Quantity : ValueObject<int>
    {
        public static Quantity Create(int value)
        {
            if (value < 0)
            {
                throw new InvalidValueObjectException(nameof(Quantity), "cannot be less than zero.");
            }

            return new Quantity { Value = value };
        }

        public static implicit operator Quantity(int value)
        {
            return Create(value);
        }
    }
}