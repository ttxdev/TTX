using System.Text.Json.Serialization;
using TTX.Dto.LootBoxes;
using TTX.ValueObjects;

namespace TTX.Notifications.LootBoxes
{
    public class OpenLootBox : INotification
    {
        [JsonPropertyName("result")] public required LootBoxResultDto Result { get; init; }

        public static OpenLootBox Create(OpenLootBoxResult result)
        {
            return new OpenLootBox { Result = LootBoxResultDto.Create(result) };
        }
    }
}