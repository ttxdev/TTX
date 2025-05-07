using System.Text.Json.Serialization;
using TTX.Dto.Players;
using TTX.Models;

namespace TTX.Dto.CreatorApplications
{
    public class CreatorApplicationDto : BaseDto
    {
        [JsonPropertyName("twitch_id")] public required string TwitchId { get; init; }

        [JsonPropertyName("ticker")] public required string Ticker { get; init; }

        [JsonPropertyName("status")] public required CreatorApplicationStatus Status { get; init; }

        [JsonPropertyName("submitter")] public required PlayerPartialDto Submitter { get; init; }

        public static CreatorApplicationDto Create(CreatorApplication app)
        {
            return new CreatorApplicationDto
            {
                Id = app.Id,
                Ticker = app.Ticker,
                TwitchId = app.TwitchId,
                Status = app.Status,
                Submitter = PlayerPartialDto.Create(app.Submitter),
                CreatedAt = app.CreatedAt,
                UpdatedAt = app.UpdatedAt
            };
        }
    }
}