using System.Text.RegularExpressions;
using TTX.Domain.Exceptions;

namespace TTX.Domain.ValueObjects;

public partial class Name : ValueObject<string>
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

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            reason = $"must be between {MaxLength} and {MinLength} characters.";
            return false;
        }

        if (!NameRegex().IsMatch(value))
        {
            reason = "can only contain letters, numbers, and underscores.";
            return false;
        }

        return true;
    }

    public static Name Create(string value)
    {
        if (!TryValidate(value, out var reason))
        {
            throw new InvalidValueObjectException<Name>(reason);
        }

        return new Name { Value = value };
    }

    [GeneratedRegex(@"^[a-zA-Z0-9_]+$")]
    public static partial Regex NameRegex();

    public static implicit operator Name(string value)
    {
        return Create(value);
    }

    public static implicit operator Name(Slug value)
    {
        return Create(value.ToString());
    }
}
