using TTX.Domain.Platforms;
using TTX.Domain.ValueObjects;

namespace TTX.Domain.Models;

public class User : Model
{
    public required Name Name { get; set; }
    public required Slug Slug { get; set; }
    public required PlatformId PlatformId { get; init; }
    public Platform Platform { get; init; } = Platform.Twitch;
    public required Uri AvatarUrl { get; set; }

    public bool Sync(PlatformUser user)
    {
        bool changed = false;
        if (user.DisplayName != Name)
        {
            Name = user.DisplayName;
            changed = true;
        }

        if (user.Username != Slug)
        {
            Slug = user.Username;
            changed = true;
        }

        if (user.AvatarUrl != AvatarUrl)
        {
            AvatarUrl = user.AvatarUrl;
            changed = true;
        }

        return changed;
    }
}
