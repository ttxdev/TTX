using TTX.Domain.ValueObjects;

namespace TTX.Domain.Models;

public class Transaction : Model
{
    public required Quantity Quantity { get; init; }
    public required Credits Value { get; init; }
    public required TransactionAction Action { get; init; }
    public required ModelId CreatorId { get; init; }
    public required ModelId PlayerId { get; init; }
    public double Total => Value * Quantity;

    public virtual Creator Creator { get; init; } = null!;
    public virtual Player Player { get; init; } = null!;

    public bool IsGain()
    {
        return Action is TransactionAction.Buy or TransactionAction.Open;
    }

    public static Transaction Create(Player actor, Creator creator, Quantity amount, TransactionAction action)
    {
        return new()
        {
            PlayerId = actor.Id,
            Player = actor,
            Creator = creator,
            CreatorId = creator.Id,
            Quantity = amount,
            Action = action,
            Value = creator.Value
        };
    }

    public static Transaction CreateBuy(Player actor, Creator creator, Quantity amount)
    {
        return Create(actor, creator, amount, TransactionAction.Buy);
    }

    public static Transaction CreateSell(Player actor, Creator creator, Quantity amount)
    {
        return Create(actor, creator, amount, TransactionAction.Sell);
    }

    public static Transaction CreateOpen(Player actor, Creator creator, Quantity amount)
    {
        return Create(actor, creator, amount, TransactionAction.Open);
    }
}
