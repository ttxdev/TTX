using System.Text.RegularExpressions;
using TTX.Exceptions;

namespace TTX.ValueObjects;

public partial class Slug : ValueObject<string>
{
    public const int MaxLength = 25;
    public const int MinLength = 3;

    public static Slug Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidValueObjectException(nameof(Slug), "cannot be null or empty.");

        if (value.Length is < MinLength or > MaxLength)
            throw new InvalidValueObjectException(nameof(Slug), $"must be between {MaxLength} and {MinLength} characters.");

        value = value.ToLower();
        if (!SlugRegex().IsMatch(value))
            throw new InvalidValueObjectException(nameof(Slug), "can only contain lowercase letters, numbers, and hyphens.");

        return new Slug { Value = value };
    }

    [GeneratedRegex(@"^[a-z0-9_]+$")]
    public static partial Regex SlugRegex();
    public static implicit operator Slug(string value) => Create(value);
    public static implicit operator Slug(Name value) => Create(value.Value.ToLower());
}
