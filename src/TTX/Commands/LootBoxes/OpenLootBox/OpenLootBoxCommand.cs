using System.Text.Json.Serialization;
using TTX.Dto.LootBoxes;
using TTX.ValueObjects;

namespace TTX.Commands.LootBoxes.OpenLootBox
{
    public readonly struct OpenLootBoxCommand : ICommand<LootBoxResultDto>
    {
        [JsonPropertyName("actor_id")] public required int ActorId { get; init; }
        [JsonPropertyName("lootbox_id")] public required int LootBoxId { get; init; }
    }
}