namespace TTX.Core.Models;

public class Share
{
    public required Creator Creator { get; set; }
    public required User User { get; set; }
    public required int Quantity { get; set; }
}