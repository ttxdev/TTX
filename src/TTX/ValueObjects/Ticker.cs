using System.Text.RegularExpressions;
using TTX.Exceptions;

namespace TTX.ValueObjects;

public partial class Ticker : ValueObject<string>
{
    public const int MaxLength = 15;
    public const int MinLength = 2;

    public static Ticker Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidValueObjectException(nameof(Ticker), "cannot be null or empty.");

        if (value.Length < MinLength || value.Length > MaxLength)
            throw new InvalidValueObjectException(nameof(Ticker), $"must be between {MaxLength} and {MinLength} characters.");

        value = value.ToUpper();
        if (!TickerRegex().IsMatch(value))
            throw new InvalidValueObjectException(nameof(Ticker), "Ticker can only contain uppercase letters.");

        return new Ticker { Value = value };
    }

    public static Ticker Create(Name name) => ToTicker(name);
    public static Ticker Create(Slug slug) => ToTicker(slug);
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
    public static implicit operator Ticker(string value) => Create(value);
}
