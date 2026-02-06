using System.Text.Json.Serialization;
using TTX.Domain.Models;

namespace TTX.App.Dto.Players;

public class PortfolioSnapshotDto
{
    [JsonPropertyName("player_id")] public required int PlayerId { get; init; }
    [JsonPropertyName("value")] public required double Value { get; init; }
    [JsonPropertyName("time")] public required DateTimeOffset Time { get; init; }

    public static PortfolioSnapshotDto Create(PortfolioSnapshot p)
    {
        return new PortfolioSnapshotDto { PlayerId = p.PlayerId, Value = p.Value, Time = p.Time };
    }
}
