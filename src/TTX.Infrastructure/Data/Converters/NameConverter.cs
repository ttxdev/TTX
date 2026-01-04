using TTX.Domain.ValueObjects;

namespace TTX.Infrastructure.Data.Converters;

public class NameConverter() : ValueObjectConverter<Name, string>(Name.Create);
