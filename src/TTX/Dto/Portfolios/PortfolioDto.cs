using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Dto.Portfolios
{
    public class PortfolioDto
    {
        [JsonPropertyName("player_id")] public required int PlayerId { get; init; }
        [JsonPropertyName("value")] public required long Value { get; init; }
        [JsonPropertyName("time")] public required DateTimeOffset Time { get; init; }

        public static PortfolioDto Create(PortfolioSnapshot p)
        {
            return new PortfolioDto
            {
                PlayerId= p.PlayerId, 
                Value = p.Value, 
                Time = p.Time
            };
        }
    }
}