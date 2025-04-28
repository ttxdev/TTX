using TTX.ValueObjects;

namespace TTX.Interfaces.Twitch
{
    public readonly struct TwitchUser
    {
        public TwitchId Id { get; init; }
        public Slug Login { get; init; }
        public Name DisplayName { get; init; }
        public Uri AvatarUrl { get; init; }
    }
}