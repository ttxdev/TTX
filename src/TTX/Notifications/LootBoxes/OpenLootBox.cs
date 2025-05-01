using TTX.Dto.LootBoxes;
using TTX.Dto.Players;
using TTX.ValueObjects;

namespace TTX.Notifications.LootBoxes
{
    public class OpenLootBox : LootBoxResultDto, INotification
    {
        public static new OpenLootBox Create(OpenLootBoxResult result)
        {
            return new OpenLootBox
            {
                LootBoxId = result.LootBox.Id,
                Player = PlayerPartialDto.Create(result.LootBox.Player),
                Result = CreatorRarityDto.Create(result.Result),
                Rarities = result.Rarities.Select(CreatorRarityDto.Create).ToArray()
            };
        }
    }
}