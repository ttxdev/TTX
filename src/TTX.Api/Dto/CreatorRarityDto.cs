using System.Text.Json.Serialization;
using TTX.Commands.LootBoxes.OpenLootBox;

namespace TTX.Api.Dto;

public class CreatorRarityDto(CreatorRarity creatorRarity)
{
    [JsonPropertyName("creator")] public CreatorPartialDto Creator { get; } = new(creatorRarity.Creator);

    [JsonPropertyName("rarity")] public Rarity Rarity { get; } = creatorRarity.Rarity;
}