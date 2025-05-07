using System.Text.Json.Serialization;
using TTX.Dto.Transactions;
using TTX.Models;

namespace TTX.Commands.Ordering.PlaceOrder
{
    public readonly struct PlaceOrderCommand : ICommand<CreatorTransactionDto>
    {
        [JsonPropertyName("actor_id")] public required int ActorId { get; init; }
        [JsonPropertyName("creator")] public required string Creator { get; init; }
        [JsonPropertyName("amount")] public required int Amount { get; init; }
        [JsonPropertyName("action")] public required TransactionAction Action { get; init; }
    }
}