using System.Text.Json.Serialization;
using TTX.Dto.Players;
using TTX.Models;

namespace TTX.Dto.Transactions
{
    public class CreatorTransactionDto(Transaction tx) : TransactionDto(tx)
    {
        [JsonPropertyName("player")] public PlayerPartialDto Player { get; } = new(tx.Player);
    }
}