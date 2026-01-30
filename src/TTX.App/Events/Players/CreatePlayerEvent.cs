using System.Text.Json.Serialization;
using TTX.App.Dto.Players;
using TTX.Domain.Models;

namespace TTX.App.Events.Players;

public record CreatePlayerEvent : BaseEvent
{
    [JsonPropertyName("player")] public required PlayerDto Player { get; init; }

    public static CreatePlayerEvent Create(Player player)
    {
        return new CreatePlayerEvent { Player = PlayerDto.Create(player) };
    }
}
