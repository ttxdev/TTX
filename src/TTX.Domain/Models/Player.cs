using TTX.Domain.Exceptions;
using TTX.Domain.Platforms;
using TTX.Domain.ValueObjects;

namespace TTX.Domain.Models;

public class Player : User
{
    public const int MaxShares = 1_000;
    public const int MinCredits = 0;
    public const int MinPortfolio = 0;
    public const int StarterCredits = 100;

    public Credits Credits { get; set; } = StarterCredits;
    public long Portfolio { get; private set; } = MinPortfolio;
    public Credits Value => Credits + Portfolio;
    public PlayerType Type { get; init; } = PlayerType.User;

    public HashSet<PortfolioSnapshot> History { get; set; } = [];
    public virtual HashSet<Transaction> Transactions { get; set; } = [];
    public virtual HashSet<LootBox> LootBoxes { get; set; } = [];

    public Share[] GetShares()
    {
        Dictionary<Creator, Share> shares = [];

        foreach (Transaction tx in Transactions)
        {
            Share share = shares.GetValueOrDefault(
                tx.Creator,
                new Share { Creator = tx.Creator, Player = this }
            );

            share.Count(tx);
            shares[tx.Creator] = share;
        }

        return [.. shares.Values.Where(share => share.Quantity > 0)];
    }

    public Transaction Give(Creator creator)
    {
        Transaction tx = Transaction.CreateOpen(this, creator, 1);
        Transactions.Add(tx);

        return tx;
    }

    public Transaction Buy(Creator creator, Quantity amount)
    {
        long value = creator.Value * amount;
        if (Credits < value)
        {
            throw new InvalidActionException("Insufficient funds.");
        }

        Share[] currentShares = GetShares();
        int currentQuantity = currentShares
            .Where(s => s.Creator.Id == creator.Id)
            .Select(s => s.Quantity.Value)
            .FirstOrDefault(0);

        if (currentQuantity + amount.Value > MaxShares)
        {
            throw new InvalidActionException($"Met max shares ({MaxShares}).");
        }

        Credits -= value;
        Transaction tx = Transaction.CreateBuy(this, creator, amount);
        Transactions.Add(tx);

        return tx;
    }

    public Transaction Sell(Creator creator, Quantity amount)
    {
        Share[] shares = GetShares();
        int quantity = shares
            .Where(s => s.Creator.Id == creator.Id)
            .Select(s => s.Quantity.Value)
            .FirstOrDefault();

        if (quantity < amount.Value)
        {
            throw new InvalidActionException("Insufficient shares.");
        }

        long value = creator.Value * amount;
        Credits += value;

        Transaction tx = Transaction.CreateSell(this, creator, amount);
        Transactions.Add(tx);

        return tx;
    }

    public PortfolioSnapshot TakePortfolioSnapshot()
    {
        Portfolio = GetShares().Aggregate(0L, (acc, share) => acc + (share.Creator.Value * share.Quantity.Value));
        PortfolioSnapshot snapshot = new() { PlayerId = Id, Player = this, Value = Portfolio };
        History.Add(snapshot);

        return snapshot;
    }

    public LootBox AddLootBox()
    {
        LootBox lootBox = LootBox.Create(this);
        LootBoxes.Add(lootBox);

        return lootBox;
    }

    public static Player Create(PlatformUser user, Credits? credits = null)
    {
        Player player = new()
        {
            Name = user.DisplayName,
            Slug = user.Username,
            AvatarUrl = user.AvatarUrl,
            PlatformId = user.Id,
            Credits = credits ?? StarterCredits
        };

        player.AddLootBox();

        return player;
    }
}
