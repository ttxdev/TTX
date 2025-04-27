using TTX.ValueObjects;

namespace TTX.Infrastructure.Data.Converters;

public class CreditsConverter() : ValueObjectConverter<Credits, long>(Credits.Create);
