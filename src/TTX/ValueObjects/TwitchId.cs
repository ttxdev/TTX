using System.Text.RegularExpressions;
using TTX.Exceptions;

namespace TTX.ValueObjects;

public partial class TwitchId : ValueObject<string>
{
    public static TwitchId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidValueObjectException(nameof(TwitchId), "cannot be null or empty.");

        if (!TwitchIdRegex().IsMatch(value))
            throw new InvalidValueObjectException(nameof(TwitchId), "must be numeric");

        return new TwitchId { Value = value };
    }

    [GeneratedRegex(@"^[0-9]+$")]
    public static partial Regex TwitchIdRegex();
    public static implicit operator TwitchId(string value) => Create(value);
    public static implicit operator TwitchId(int value) => Create(value.ToString());
    public static implicit operator int(TwitchId twitchId) => int.Parse(twitchId.Value);
}
