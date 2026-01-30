using System.Text.Json.Serialization;
using TTX.App.Dto.Players;
using TTX.Domain.Models;

namespace TTX.App.Events.Players;

public record UpdatePlayerPortfolioEvent : BaseEvent
{
    [JsonPropertyName("player")] public required PlayerPartialDto Player { get; init; }

    public static UpdatePlayerPortfolioEvent Create(PortfolioSnapshot p)
    {
        return new UpdatePlayerPortfolioEvent { Player = PlayerPartialDto.Create(p.Player) };
    }
}
