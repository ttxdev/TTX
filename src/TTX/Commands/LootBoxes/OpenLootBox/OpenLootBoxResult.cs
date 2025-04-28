using System.Collections.Immutable;

namespace TTX.Commands.LootBoxes.OpenLootBox
{
    public readonly struct OpenLootBoxResult
    {
        public required CreatorRarity Result { get; init; }
        public required ImmutableArray<CreatorRarity> Rarities { get; init; }
    }
}