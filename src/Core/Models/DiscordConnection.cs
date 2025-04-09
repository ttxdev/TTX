namespace TTX.Core.Models;

public class DiscordConnection
{
    public required string id { get; set; }
    public required string type { get; set; }
    public required string name { get; set; }
    public required bool verified { get; set; }
    public required int visibility { get; set; }
    public required bool friend_sync { get; set; }
    public required int metadata_visibility { get; set; }
    public required bool show_activity { get; set; }
    public required bool two_way_link { get; set; }

}