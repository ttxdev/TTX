using System.Text.Json.Serialization;
using TTX.Domain.Models;

namespace TTX.App.Services.Players;

public record PlaceOrderRequest
{
    [JsonPropertyName("actor_id")] public required int ActorId { get; init; }
    [JsonPropertyName("creator_slug")] public required string CreatorSlug { get; init; }
    [JsonPropertyName("quantity")] public required int Quantity { get; init; }
    [JsonPropertyName("action")] public required TransactionAction Action { get; init; }
}
