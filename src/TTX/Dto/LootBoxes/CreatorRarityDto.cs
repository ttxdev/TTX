using System.Text.Json.Serialization;
using TTX.Dto.Creators;
using TTX.ValueObjects;

namespace TTX.Dto.LootBoxes
{
    public class CreatorRarityDto(CreatorRarity creatorRarity)
    {
        [JsonPropertyName("creator")] public CreatorPartialDto Creator { get; } = new(creatorRarity.Creator);

        [JsonPropertyName("rarity")] public Rarity Rarity { get; } = creatorRarity.Rarity;
    }
}