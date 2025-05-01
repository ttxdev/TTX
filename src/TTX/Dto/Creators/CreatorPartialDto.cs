using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Dto.Creators
{
    public class CreatorPartialDto : UserDto
    {
        [JsonPropertyName("ticker")] public required string Ticker { get; init; }

        [JsonPropertyName("value")] public required long Value { get; init; }

        [JsonPropertyName("stream_status")] public required StreamStatusDto StreamStatus { get; init; }

        [JsonPropertyName("history")] public required VoteDto[] History { get; init; }

        public static CreatorPartialDto Create(Creator creator)
        {
            return new CreatorPartialDto
            {
                Id = creator.Id,
                Name = creator.Name,
                Slug = creator.Slug,
                Ticker = creator.Ticker,
                TwitchId = creator.TwitchId,
                Value = creator.Value,
                StreamStatus = StreamStatusDto.Create(creator.StreamStatus),
                History = creator.History.Select(VoteDto.Create).ToArray(),
                AvatarUrl = creator.AvatarUrl.ToString(),
                UpdatedAt = creator.UpdatedAt,
                CreatedAt = creator.CreatedAt
            };
        }
    }
}