using TTX.Domain.ValueObjects;

namespace TTX.Domain.Models;

public class User : Model
{
    public required Name Name { get; set; }
    public required Slug Slug { get; set; }
    public required PlatformId PlatformId { get; init; }
    public Platform Platform { get; init; } = Platform.Twitch;
    public required Uri AvatarUrl { get; set; }

    public bool Sync(Name? name = null, Slug? slug = null, Uri? avatarUrl = null)
    {
        bool changed = false;
        if (name is not null && name != Name)
        {
            Name = name;
            changed = true;
        }

        if (slug is not null && slug != Slug)
        {
            Slug = slug;
            changed = true;
        }

        if (avatarUrl is not null && avatarUrl != AvatarUrl)
        {
            AvatarUrl = avatarUrl;
            changed = true;
        }

        return changed;
    }
}
