using System.ComponentModel.DataAnnotations;
using TTX.Core.Exceptions;

namespace TTX.Core.Models;

public enum TransactionAction
{
    Buy,
    Sell
}

public class Transaction : ModelBase, IValidatableObject
{
    public required int Quantity { get; set; }
    public required long Value { get; set; }
    public required TransactionAction Action { get; set; }
    public required int CreatorId { get; set; }
    public required int UserId { get; set; }
    public User User { get; set; } = null!;
    public Creator Creator { get; set; } = null!;
    public bool IsBuy() => Action == TransactionAction.Buy;
    public bool IsSell() => Action == TransactionAction.Sell;

    public static Transaction Create(User user, Creator creator, int amount, TransactionAction action)
    {
        var tx = new Transaction
        {
            UserId = user.Id,
            CreatorId = creator.Id,
            User = user,
            Creator = creator,
            Quantity = amount,
            Action = action,
            Value = creator.Value
        };

        var valResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(tx);
        if (!Validator.TryValidateObject(user, validationContext, valResults, true))
            throw new ModelValidationException(valResults);

        return tx;
    }

    public static Transaction CreateBuy(User user, Creator creator, int amount) => Create(user, creator, amount, TransactionAction.Buy);
    public static Transaction CreateSell(User user, Creator creator, int amount) => Create(user, creator, amount, TransactionAction.Sell);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Quantity <= 0)
            yield return new ValidationResult("Quantity must be greater than 0.", [nameof(Quantity)]);

        if (Value <= 0)
            yield return new ValidationResult("Value must be greater than 0.", [nameof(Value)]);
    }
}