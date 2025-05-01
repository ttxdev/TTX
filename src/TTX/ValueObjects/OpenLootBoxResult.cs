using System.Collections.Immutable;
using TTX.Models;

namespace TTX.ValueObjects
{
    public readonly struct OpenLootBoxResult
    {
        public required LootBox LootBox { get; init; }
        public required CreatorRarity Result { get; init; }
        public required ImmutableArray<CreatorRarity> Rarities { get; init; }
    }
}