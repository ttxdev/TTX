using TTX.Domain.Models;

namespace TTX.Domain.ValueObjects;

public readonly struct OpenLootBoxResult
{
    public required LootBox LootBox { get; init; }
    public required CreatorRarity Result { get; init; }
    public required CreatorRarity[] Rarities { get; init; }
}
