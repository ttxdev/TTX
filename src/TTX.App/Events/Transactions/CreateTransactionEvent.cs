using System.Text.Json.Serialization;
using TTX.App.Dto.Transactions;
using TTX.Domain.Models;

namespace TTX.App.Events.Transactions;

public record CreateTransactionEvent : BaseEvent
{
    [JsonPropertyName("transaction")] public required CreatorTransactionDto Transaction { get; init; }

    public static CreateTransactionEvent Create(Transaction tx)
    {
        return new CreateTransactionEvent { Transaction = CreatorTransactionDto.Create(tx) };
    }
}
