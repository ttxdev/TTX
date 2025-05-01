using TTX.Dto.Players;
using TTX.Models;

namespace TTX.Notifications.Players
{
    public class CreatePlayer(Player player) : PlayerDto(player), INotification;
}