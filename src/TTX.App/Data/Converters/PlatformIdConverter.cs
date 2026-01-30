using TTX.Domain.ValueObjects;

namespace TTX.App.Data.Converters;

public class PlatformIdConverter() : ValueObjectConverter<PlatformId, string>(PlatformId.Create);
