using System.Text.RegularExpressions;
using TTX.Domain.Exceptions;

namespace TTX.Domain.ValueObjects;

public partial class Ticker : ValueObject<string>
{
    public const int MaxLength = 15;
    public const int MinLength = 2;

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

        value = value.ToUpper();
        if (!TickerRegex().IsMatch(value))
        {
            reason = "Ticker can only contain uppercase letters.";
            return false;
        }

        return true;
    }

    public static Ticker Create(string value)
    {
        if (!TryValidate(value, out var reason))
        {
            throw new InvalidValueObjectException<Ticker>(reason);
        }

        return new Ticker { Value = value };
    }

    public static Ticker Create(Name name)
    {
        if (!TryValidate(name.Value, out var reason))
        {
            throw new InvalidValueObjectException<Ticker>(reason);
        }

        return ToTicker(name);
    }

    public static Ticker Create(Slug slug)
    {
        if (!TryValidate(slug.Value, out var reason))
        {
            throw new InvalidValueObjectException<Ticker>(reason);
        }

        return ToTicker(slug);
    }

    private static Ticker ToTicker(string value)
    {
        string abbrev = value
            .Split('_')
            .Select(word => word[0].ToString().ToUpper())
            .Aggregate(string.Empty, (current, next) => current + next);

        return Create(abbrev);
    }

    [GeneratedRegex(@"^[A-Z0-9+$:]+$")]
    public static partial Regex TickerRegex();

    public static implicit operator Ticker(string value)
    {
        return Create(value);
    }
}
