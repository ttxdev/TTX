using TTX.Domain.ValueObjects;

namespace TTX.App.Data.Converters;

public class CreditsConverter() : ValueObjectConverter<Credits, long>(Credits.Create);
