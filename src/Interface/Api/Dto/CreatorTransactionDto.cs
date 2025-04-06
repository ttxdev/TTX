using System.Text.Json.Serialization;
using TTX.Core.Models;

namespace TTX.Interface.Api.Dto;

public class CreatorTransactionDto(Transaction tx) : BaseDto<Transaction>(tx)
{
    [JsonPropertyName("quantity")]
    public int Quantity { get; } = tx.Quantity;
    [JsonPropertyName("value")]
    public long Value { get; } = tx.Value;
    [JsonPropertyName("action")]
    public TransactionAction Action { get; } = tx.Action;
    [JsonPropertyName("creator")]
    public UserPartialDto User { get; } = new UserPartialDto(tx.User);
}