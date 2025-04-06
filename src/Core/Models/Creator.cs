using System.ComponentModel.DataAnnotations;
using TTX.Core.Exceptions;

namespace TTX.Core.Models;

public class Creator : ModelBase, IValidatableObject
{
    public const long MIN_VALUE = 1;
    public required string Name { get; set; }
    [MinLength(4)]
    [MaxLength(25)]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Slug must be alphanumeric and can include underscores.")]
    public required string Slug { get; set; }
    [RegularExpression(@"^[A-Z]{2,5}$", ErrorMessage = "Ticker must be 2-5 uppercase letters.")]
    public required string Ticker { get; set; }
    public required string AvatarUrl { get; set; }
    public string Url => $"https://twitch.tv/{Slug}";
    public long Value { get; private set; } = 0;
    public StreamStatus StreamStatus { get; set; } = new StreamStatus();
    public List<Transaction> Transactions { get; set; } = [];

    public Share[] GetShares()
    {
        var shares = new Dictionary<int, Share>();
        foreach (var tx in Transactions)
        {
            var share = shares.GetValueOrDefault(tx.Creator.Id, new Share
            {
                Creator = this,
                User = tx.User,
                Quantity = 0
            });

            if (tx.IsBuy()) share.Quantity += tx.Quantity;
            else share.Quantity -= tx.Quantity;
        }

        return [.. shares.Values.Where(s => s.Quantity > 0)];
    }

    public Vote CreateVote(long value)
    {
        Value = Math.Max(MIN_VALUE, Value + value);
        return new Vote
        {
            CreatorId = Id,
            Value = value,
            Time = DateTimeOffset.UtcNow
        };
    }

    public static Creator Create(TwitchUser tUser, string ticker)
    {
        var creator = new Creator()
        {
            Name = tUser.DisplayName,
            Slug = tUser.Login,
            AvatarUrl = tUser.AvatarUrl,
            Ticker = ticker,
        };

        var validatorResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(creator);
        if (!Validator.TryValidateObject(creator, validationContext, validatorResults, true))
            throw new ModelValidationException(validatorResults);

        return creator;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Value < MIN_VALUE)
            yield return new ValidationResult($"Value must be greater than or equal to {MIN_VALUE}.", [nameof(Value)]);
    }
}