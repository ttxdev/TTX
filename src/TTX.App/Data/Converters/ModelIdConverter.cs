using TTX.Domain.ValueObjects;

namespace TTX.App.Data.Converters;

public class ModelIdConverter() : ValueObjectConverter<ModelId, int>(ModelId.Create);
