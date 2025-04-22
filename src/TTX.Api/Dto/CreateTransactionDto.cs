using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Api.Dto;

public class CreateTransactionDto
{
    [JsonPropertyName("creator")]
    public required string CreatorSlug { get; init; }
    [JsonPropertyName("action")]
    public required TransactionAction Action { get; init; }
    [JsonPropertyName("amount")]
    public required int Amount { get; init; }
}
