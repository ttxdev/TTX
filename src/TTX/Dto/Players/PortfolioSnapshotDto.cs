using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Dto.Players
{
    public class PortfolioSnapshotDto
    {
        [JsonPropertyName("player_id")] public required int PlayerId { get; init; }
        [JsonPropertyName("value")] public required long Value { get; init; }
        [JsonPropertyName("time")] public required DateTimeOffset Time { get; init; }

        public static PortfolioSnapshotDto Create(PortfolioSnapshot p)
        {
            return new PortfolioSnapshotDto { PlayerId = p.PlayerId, Value = p.Value, Time = p.Time };
        }
    }
}