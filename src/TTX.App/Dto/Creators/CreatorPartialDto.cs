using System.Text.Json.Serialization;
using TTX.Domain.Models;

namespace TTX.App.Dto.Creators;

public class CreatorPartialDto : UserDto
{
    [JsonPropertyName("ticker")] public required string Ticker { get; init; }

    [JsonPropertyName("value")] public required double Value { get; init; }

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
            PlatformId = creator.PlatformId,
            Platform = creator.Platform,
            Value = creator.Value,
            StreamStatus = StreamStatusDto.Create(creator.StreamStatus),
            History = creator.History.Select(VoteDto.Create).ToArray(),
            AvatarUrl = creator.AvatarUrl.ToString(),
            UpdatedAt = creator.UpdatedAt,
            CreatedAt = creator.CreatedAt
        };
    }
}
