using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Api.Dto;

public class PlayerTransactionDto(Transaction tx) : TransactionDto(tx)
{
    [JsonPropertyName("creator")]
    public CreatorPartialDto Creator { get; } = new CreatorPartialDto(tx.Creator);
}