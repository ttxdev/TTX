using TTX.ValueObjects;

namespace TTX.Infrastructure.Data.Converters
{
    public class QuantityConverter() : ValueObjectConverter<Quantity, int>(Quantity.Create);
}