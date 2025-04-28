using TTX.ValueObjects;

namespace TTX.Infrastructure.Data.Converters
{
    public class ModelIdConverter() : ValueObjectConverter<ModelId, int>(ModelId.Create);
}