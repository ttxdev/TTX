using System.Text.Json.Serialization;
using TTX.Dto.Creators;
using TTX.Models;

namespace TTX.Dto.Transactions
{
    public class PlayerTransactionDto(Transaction tx) : TransactionDto(tx)
    {
        [JsonPropertyName("creator")] public CreatorPartialDto Creator { get; } = new(tx.Creator);
    }
}