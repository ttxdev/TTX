using TTX.Commands.LootBoxes.OpenLootBox;

namespace TTX.Notifications.LootBoxes;

public class OpenLootBox : INotification
{
  public required OpenLootBoxResult Result { get; init; }
}