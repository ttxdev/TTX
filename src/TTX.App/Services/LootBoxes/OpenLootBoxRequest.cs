using TTX.Domain.ValueObjects;

namespace TTX.App.Services.LootBoxes;

public record OpenLootBoxRequest
{
    public required ModelId PlayerId { get; init; }
    public required ModelId LootBoxId { get; init; }
}
