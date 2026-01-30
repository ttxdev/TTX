using System.Text.RegularExpressions;
using TTX.Domain.Exceptions;

namespace TTX.Domain.ValueObjects;

public partial class Slug : ValueObject<string>
{
    public const int MaxLength = 25;
    public const int MinLength = 3;

    public static bool TryValidate(string value, out string reason)
    {
        reason = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            reason = "cannot be null or empty.";
            return false;
        }

        if (value.Length is < MinLength or > MaxLength)
        {
            reason = $"must be between {MaxLength} and {MinLength} characters.";
            return false;
        }

        value = value.ToLower();
        if (!SlugRegex().IsMatch(value))
        {
            reason = "can only contain lowercase letters, numbers, and hyphens.";
            return false;
        }

        return true;
    }

    public static Slug Create(string value)
    {
        if (!TryValidate(value, out var reason))
        {
            throw new InvalidValueObjectException<Slug>(reason);
        }

        return new Slug { Value = value };
    }

    [GeneratedRegex(@"^[a-z0-9_]+$")]
    public static partial Regex SlugRegex();

    public static implicit operator Slug(string value)
    {
        return Create(value);
    }

    public static implicit operator Slug(Name value)
    {
        return Create(value.Value.ToLower());
    }
}
