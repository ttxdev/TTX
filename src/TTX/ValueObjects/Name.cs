using System.Text.RegularExpressions;
using TTX.Exceptions;

namespace TTX.ValueObjects
{
    public partial class Name : ValueObject<string>
    {
        public const int MaxLength = 25;
        public const int MinLength = 3;

        public static Name Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidValueObjectException(nameof(Name), "cannot be null or empty.");
            }

            if (value.Length < MinLength || value.Length > MaxLength)
            {
                throw new InvalidValueObjectException(nameof(Name),
                    $"must be between {MaxLength} and {MinLength} characters.");
            }

            if (!NameRegex().IsMatch(value))
            {
                throw new InvalidValueObjectException(nameof(Name),
                    "can only contain letters, numbers, and underscores.");
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
}