using TTX.Domain.ValueObjects;

namespace TTX.Domain.Platforms;

public record PlatformUser
{
    public required PlatformId Id { get; init; }
    public required Slug Username { get; init; }
    public required Name DisplayName { get; init; }
    public required Uri AvatarUrl { get; init; }
}
