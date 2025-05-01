using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Dto.Transactions
{
    public class TransactionDto(Transaction tx) : BaseDto(tx)
    {
        [JsonPropertyName("quantity")] public int Quantity { get; } = tx.Quantity;

        [JsonPropertyName("value")] public long Value { get; } = tx.Value;

        [JsonPropertyName("action")] public TransactionAction Action { get; } = tx.Action;

        [JsonPropertyName("creator_id")] public int CreatorId { get; } = tx.Creator.Id;

        [JsonPropertyName("player_id")] public int PlayerId { get; } = tx.Player.Id;
    }
}
