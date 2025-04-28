using TTX.ValueObjects;

namespace TTX.Infrastructure.Data.Converters
{
    public class TwitchIdConverter() : ValueObjectConverter<TwitchId, string>(TwitchId.Create);
}