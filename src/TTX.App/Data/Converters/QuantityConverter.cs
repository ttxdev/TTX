using TTX.Domain.ValueObjects;

namespace TTX.App.Data.Converters;

public class QuantityConverter() : ValueObjectConverter<Quantity, int>(Quantity.Create);
