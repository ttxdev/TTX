using System.Collections.Immutable;
using TTX.ValueObjects;

namespace TTX.Models
{
    public class Creator : User
    {
        public const int MinValue = 1;
        public const int StarterValue = 1;
        public required Ticker Ticker { get; init; }
        public Credits Value { get; private set; } = StarterValue;
        public StreamStatus StreamStatus { get; init; } = new();
        public HashSet<Transaction> Transactions { get; init; } = [];
        public HashSet<Vote> History { get; set; } = [];

        public ImmutableArray<Share> GetShares()
        {
            Dictionary<ModelId, Share> shares = new();
            foreach (Transaction tx in Transactions)
            {
                Share share = shares.GetValueOrDefault(tx.Player.Id, new Share { Creator = this, Player = tx.Player });

                share.Count(tx);
                shares[tx.Player.Id] = share;
            }

            return [.. shares.Values.Where(share => share.Quantity > 0)];
        }

        public void RecordTransaction(Transaction tx)
        {
            Transactions.Add(tx);
        }

        public Vote ApplyNetChange(long netChange)
        {
            Value = Math.Max(MinValue, Value + netChange);

            return new Vote { Creator = this, CreatorId = Id, Value = Value, Time = DateTimeOffset.UtcNow };
        }

        public static Creator Create(Name name, Slug slug, TwitchId twitchId, Uri avatarUrl, Ticker ticker,
            Credits? value = null)
        {
            Creator creator = new()
            {
                Name = name,
                Slug = slug,
                TwitchId = twitchId,
                AvatarUrl = avatarUrl,
                Ticker = ticker,
                Value = value ?? StarterValue
            };

            return creator;
        }
    }
}