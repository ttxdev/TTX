using System.Text.Json.Serialization;
using TTX.Domain.Models;

namespace TTX.App.Dto.Creators;

public class CreatorOptOutDto : BaseDto
{
    [JsonPropertyName("platform_id")] public required string PlatformId { get; init; }

    [JsonPropertyName("platform")] public required Platform Platform { get; init; }

    public static CreatorOptOutDto Create(CreatorOptOut opt)
    {
        return new CreatorOptOutDto
        {
            Id = opt.Id,
            Platform = opt.Platform,
            PlatformId = opt.PlatformId,
            CreatedAt = opt.CreatedAt,
            UpdatedAt = opt.UpdatedAt
        };
    }
}
