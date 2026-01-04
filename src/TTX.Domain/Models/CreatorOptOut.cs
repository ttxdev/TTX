using TTX.Domain.ValueObjects;

namespace TTX.Domain.Models;

public class CreatorOptOut : Model
{
    public required PlatformId PlatformId { get; init; }
    public required Platform Platform { get; init; }
    public string Reason { get; init; } = string.Empty;

    public static CreatorOptOut Create(Creator creator, string? reason = null)
    {
        return new()
        {
            PlatformId = creator.PlatformId,
            Platform = creator.Platform,
            Reason = reason ?? string.Empty
        };
    }
}
