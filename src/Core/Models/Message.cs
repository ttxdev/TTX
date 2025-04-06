namespace TTX.Core.Models;

public class Message
{
    public required Creator Creator { get; set; }
    public required string Content { get; set; }
}