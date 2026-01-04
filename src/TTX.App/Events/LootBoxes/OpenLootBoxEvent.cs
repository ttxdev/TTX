using System.Text.Json.Serialization;
using TTX.Domain.ValueObjects;
using TTX.App.Dto.LootBoxes;

namespace TTX.App.Events.LootBoxes;

public class OpenLootBoxEvent : IEvent
{
    [JsonPropertyName("result")] public required LootBoxResultDto Result { get; init; }

    public static OpenLootBoxEvent Create(OpenLootBoxResult result, ModelId transactionId)
    {
        return new OpenLootBoxEvent { Result = LootBoxResultDto.Create(result, transactionId) };
    }
}
