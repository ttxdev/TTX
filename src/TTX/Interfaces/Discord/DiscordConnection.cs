using System.Text.Json.Serialization;

namespace TTX.Interfaces.Discord;

public readonly struct DiscordConnection
{
    [JsonPropertyName("id")]
    public string Id { get; init; }
    [JsonPropertyName("type")]
    public string Type { get; init; }
    [JsonPropertyName("name")]
    public string Name { get; init; }
    [JsonPropertyName("verified")]
    public bool Verified { get; init; }
    [JsonPropertyName("visibility")]
    public int Visibility { get; init; }
    [JsonPropertyName("friend_sync")]
    public bool FriendSync { get; init; }
    [JsonPropertyName("metadata_visibility")]
    public int MetadataVisibility { get; init; }
    [JsonPropertyName("show_activity")]
    public bool ShowActivity { get; init; }
    [JsonPropertyName("two_way_link")]
    public bool TwoWayLink { get; init; }
}