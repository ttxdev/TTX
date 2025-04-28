using TTX.ValueObjects;

namespace TTX.Models
{
    public class User : Model
    {
        public required Name Name { get; set; }
        public required Slug Slug { get; set; }
        public required TwitchId TwitchId { get; init; }
        public required Uri AvatarUrl { get; set; }

        public bool Sync(Name name, Slug slug, Uri avatarUrl)
        {
            bool isChanged = false;
            if (name != Name)
            {
                Name = name;
                isChanged = true;
            }

            if (slug != Slug)
            {
                Slug = slug;
                isChanged = true;
            }

            if (avatarUrl != AvatarUrl)
            {
                AvatarUrl = avatarUrl;
                isChanged = true;
            }

            return isChanged;
        }
    }
}