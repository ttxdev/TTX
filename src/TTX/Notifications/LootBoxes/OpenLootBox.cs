using TTX.Dto.LootBoxes;
using TTX.ValueObjects;

namespace TTX.Notifications.LootBoxes
{
    public class OpenLootBox(OpenLootBoxResult result) : LootBoxResultDto(result), INotification;
}