using System.Text.Json.Serialization;
using TTX.Dto.Creators;
using TTX.Dto.Players;
using TTX.Models;

namespace TTX.Dto.LootBoxes
{
    public class LootBoxDto : BaseDto
    {
        [JsonPropertyName("is_open")] public required bool IsOpen { get; init; }

        [JsonPropertyName("result")] public required CreatorPartialDto? Result { get; init; }

        [JsonPropertyName("player")] public required PlayerPartialDto Player { get; init; }

        public static LootBoxDto Create(LootBox lootBox)
        {
            return new LootBoxDto
            {
                Id = lootBox.Id,
                IsOpen = lootBox.IsOpen,
                Result = lootBox.Result != null ? CreatorPartialDto.Create(lootBox.Result) : null,
                Player = PlayerPartialDto.Create(lootBox.Player),
                CreatedAt = lootBox.CreatedAt,
                UpdatedAt = lootBox.UpdatedAt
            };
        }
    }
}