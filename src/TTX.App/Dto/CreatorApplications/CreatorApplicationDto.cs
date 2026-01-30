using System.Text.Json.Serialization;
using TTX.App.Dto.Players;
using TTX.Domain.Models;

namespace TTX.App.Dto.CreatorApplications;

public class CreatorApplicationDto : BaseDto
{
    [JsonPropertyName("platform")] public required Platform Platform { get; init; }

    [JsonPropertyName("platform_id")] public required string PlatformId { get; init; }

    [JsonPropertyName("ticker")] public required string Ticker { get; init; }

    [JsonPropertyName("status")] public required CreatorApplicationStatus Status { get; init; }

    [JsonPropertyName("submitter")] public required PlayerPartialDto Submitter { get; init; }

    public static CreatorApplicationDto Create(CreatorApplication app)
    {
        return new CreatorApplicationDto
        {
            Id = app.Id,
            Ticker = app.Ticker,
            Platform = app.Platform,
            PlatformId = app.PlatformId,
            Status = app.Status,
            Submitter = PlayerPartialDto.Create(app.Submitter),
            CreatedAt = app.CreatedAt,
            UpdatedAt = app.UpdatedAt
        };
    }
}
