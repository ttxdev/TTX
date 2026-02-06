using System.Collections.Immutable;
using TTX.Domain.Platforms;
using TTX.Domain.ValueObjects;

namespace TTX.Domain.Models;

public class Creator : User
{
    public const int MinValue = 1;
    public const int StarterValue = 1;
    public required Ticker Ticker { get; set; }
    public Credits Value { get; set; } = StarterValue;
    public StreamStatus StreamStatus { get; init; } = new();

    public virtual ICollection<Transaction> Transactions { get; init; } = [];
    public virtual ICollection<Vote> History { get; set; } = [];

    public ImmutableArray<Share> GetShares()
    {
        Dictionary<ModelId, Share> shares = [];
        foreach (Transaction tx in Transactions)
        {
            Share share = shares.GetValueOrDefault(tx.Player.Id, new Share { Creator = this, Player = tx.Player });

            share.Count(tx);
            shares[tx.Player.Id] = share;
        }

        return [.. shares.Values.Where(share => share.Quantity > 0)];
    }

    public Vote ApplyNetChange(double netChange)
    {
        Value = Math.Max(MinValue, Value + netChange);

        return new() { Creator = this, CreatorId = Id, Value = Value };
    }

    public static Creator Create(PlatformUser user, Ticker ticker, Platform platform)
    {
        return new()
        {
            Ticker = ticker,
            Platform = platform,
            PlatformId = user.Id,
            Name = user.DisplayName,
            Slug = user.Username,
            AvatarUrl = user.AvatarUrl
        };
    }
}
