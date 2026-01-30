using TTX.Domain.ValueObjects;

namespace TTX.App.Data.Converters;

public class NameConverter() : ValueObjectConverter<Name, string>(Name.Create);
