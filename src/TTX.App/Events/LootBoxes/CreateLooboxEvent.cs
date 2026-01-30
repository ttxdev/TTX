using System.Text.Json.Serialization;
using TTX.App.Dto.LootBoxes;
using TTX.Domain.Models;

namespace TTX.App.Events.LootBoxes;

public record CreateLooboxEvent : BaseEvent
{
    [JsonPropertyName("loot_box")] public required LootBoxDto LootBox { get; init; }

    public static CreateLooboxEvent Create(LootBox lootBox)
    {
        return new CreateLooboxEvent
        {
            LootBox = LootBoxDto.Create(lootBox)
        };
    }
}
