using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Dto.Transactions
{
    public class TransactionDto(Transaction tx) : BaseDto(tx)
    {
        [JsonPropertyName("quantity")] public int Quantity { get; } = tx.Quantity;

        [JsonPropertyName("value")] public long Value { get; } = tx.Value;

        [JsonPropertyName("action")] public TransactionAction Action { get; } = tx.Action;
    }
}