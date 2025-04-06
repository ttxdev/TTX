namespace TTX.Core.Models;

public class TwitchUser
{
    public required string Id { get; set; }
    public required string Login { get; set; }
    public required string DisplayName { get; set; }
    public required string AvatarUrl { get; set; }
}