using System.Text.RegularExpressions;
using TTX.Domain.Exceptions;

namespace TTX.Domain.ValueObjects;

public partial class PlatformId : ValueObject<string>
{
    public static PlatformId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidValueObjectException<PlatformId>("cannot be null or empty.");
        }

        if (!TwitchIdRegex().IsMatch(value))
        {
            throw new InvalidValueObjectException<PlatformId>("must be numeric");
        }

        return new PlatformId { Value = value };
    }

    [GeneratedRegex(@"^[0-9]+$")]
    public static partial Regex TwitchIdRegex();

    public static implicit operator PlatformId(string value)
    {
        return Create(value);
    }

    public static implicit operator PlatformId(int value)
    {
        return Create(value.ToString());
    }

    public static implicit operator int(PlatformId twitchId)
    {
        return int.Parse(twitchId.Value);
    }
}
