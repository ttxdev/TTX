using System.Text.Json.Serialization;
using TTX.Domain.Models;

namespace TTX.App.Dto.Transactions;

public class TransactionDto : BaseDto
{
    [JsonPropertyName("quantity")] public required int Quantity { get; init; }

    [JsonPropertyName("value")] public required double Value { get; init; }

    [JsonPropertyName("action")] public required TransactionAction Action { get; init; }

    [JsonPropertyName("creator_id")] public required int CreatorId { get; init; }

    [JsonPropertyName("player_id")] public required int PlayerId { get; init; }
}
