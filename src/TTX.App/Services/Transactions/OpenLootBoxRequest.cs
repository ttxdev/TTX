using TTX.Domain.ValueObjects;

namespace TTX.App.Services.Transactions;

public record OpenLootBoxRequest
{
    public required ModelId PlayerId { get; init; }
    public required ModelId LootBoxId { get; init; }
}
