using System.Text.Json.Serialization;
using TTX.Dto.Players;
using TTX.Models;

namespace TTX.Notifications.Players
{
    public class CreatePlayer : INotification
    {
        [JsonPropertyName("player")] public required PlayerDto Player { get; init; }

        public static CreatePlayer Create(Player player)
        {
            return new CreatePlayer { Player = PlayerDto.Create(player) };
        }
    }
}