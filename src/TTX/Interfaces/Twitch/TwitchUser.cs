using TTX.ValueObjects;

namespace TTX.Interfaces.Twitch
{
    public class TwitchUser
    {
        public required TwitchId Id { get; init; }
        public required Slug Login { get; init; }
        public required Name DisplayName { get; init; }
        public required Uri AvatarUrl { get; init; }
    }
}