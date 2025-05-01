using System.Text.Json.Serialization;
using TTX.Dto.Players;
using TTX.ValueObjects;

namespace TTX.Dto.LootBoxes
{
    public class LootBoxResultDto(OpenLootBoxResult result)
    {
        [JsonPropertyName("lootbox_id")] public ModelId LootBoxId { get; } = result.LootBox.Id;
        
        [JsonPropertyName("player")] public PlayerPartialDto Player { get; } = new(result.LootBox.Player);

        [JsonPropertyName("result")] public CreatorRarityDto Result { get; } = new(result.Result);

        [JsonPropertyName("rarities")]
        public CreatorRarityDto[] Rarities { get; } = [.. result.Rarities.Select(x => new CreatorRarityDto(x))];
    }
}