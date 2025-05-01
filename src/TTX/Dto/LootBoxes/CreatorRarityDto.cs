using System.Text.Json.Serialization;
using TTX.Dto.Creators;
using TTX.ValueObjects;

namespace TTX.Dto.LootBoxes
{
    public class CreatorRarityDto
    {
        [JsonPropertyName("creator")] public required CreatorPartialDto Creator { get; init; }
        [JsonPropertyName("rarity")] public required Rarity Rarity { get; init; }

        public static CreatorRarityDto Create(CreatorRarity rarity)
        {
            return new CreatorRarityDto { Creator = CreatorPartialDto.Create(rarity.Creator), Rarity = rarity.Rarity };
        }
    }
}