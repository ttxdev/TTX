namespace TTX.Core.Models;

public class Vote
{
    public required int CreatorId { get; set; }
    public required long Value { get; set; }
    public required DateTimeOffset Time { get; set; }
}
