using System.Text.Json.Serialization;
using TTX.Dto.Players;
using TTX.Models;

namespace TTX.Notifications.Players
{
    public class UpdatePlayerPortfolio : INotification
    {
        [JsonPropertyName("player")] public required PlayerPartialDto Player { get; init; }

        public static UpdatePlayerPortfolio Create(PortfolioSnapshot p)
        {
            return new UpdatePlayerPortfolio { Player = PlayerPartialDto.Create(p.Player) };
        }
    }
}