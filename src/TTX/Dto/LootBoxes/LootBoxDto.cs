using System.Text.Json.Serialization;
using TTX.Dto.Creators;
using TTX.Dto.Players;
using TTX.Models;

namespace TTX.Dto.LootBoxes
{
    public class LootBoxDto(LootBox lootBox) : BaseDto(lootBox)
    {
        [JsonPropertyName("is_open")] public bool IsOpen { get; } = lootBox.IsOpen;

        [JsonPropertyName("result")]
        public CreatorPartialDto? Result { get; } = lootBox.Result is not null
            ? new CreatorPartialDto(lootBox.Result!)
            : null;

        [JsonPropertyName("player")] public PlayerPartialDto Player { get; } = new(lootBox.Player);
    }
}