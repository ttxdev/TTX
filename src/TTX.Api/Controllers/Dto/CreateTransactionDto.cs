using System.Text.Json.Serialization;
using TTX.Domain.Models;

namespace TTX.Api.Controllers.Dto;

public class CreateTransactionDto
{
    [JsonPropertyName("creator")] public required string CreatorSlug { get; init; }
    [JsonPropertyName("action")] public required TransactionAction Action { get; init; }
    [JsonPropertyName("amount")] public required int Quantity { get; init; }
}
