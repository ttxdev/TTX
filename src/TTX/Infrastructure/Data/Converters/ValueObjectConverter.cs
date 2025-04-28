using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TTX.ValueObjects;

namespace TTX.Infrastructure.Data.Converters
{
    public class ValueObjectConverter<T, V>(
        Func<V, T> createFunc
    ) : ValueConverter<T, V>(
        v => v.Value,
        v => createFunc(v)
    ) where T : ValueObject<V> where V : notnull;
}