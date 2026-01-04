using TTX.Domain.ValueObjects;

namespace TTX.Infrastructure.Data.Converters;

public class PlatformIdConverter() : ValueObjectConverter<PlatformId, string>(PlatformId.Create);
