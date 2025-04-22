using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Api.Dto;

public class CreatorTransactionDto(Transaction tx) : TransactionDto(tx)
{
    [JsonPropertyName("player")]
    public PlayerPartialDto Player { get; } = new PlayerPartialDto(tx.Player);
}