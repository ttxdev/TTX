using System.Collections.Immutable;
using TTX.Exceptions;
using TTX.ValueObjects;

namespace TTX.Models;

public class Player : User
{
    public const int MaxShares = 1_000;
    public const int MinCredits = 0;
    public const int StarterCredits = 100;
    public Credits Credits { get; private set; } = StarterCredits;
    public PlayerType Type { get; init; } = PlayerType.User;
    public HashSet<Transaction> Transactions { get; init; } = [];
    public HashSet<LootBox> LootBoxes { get; init; } = [];

    public ImmutableArray<Share> GetShares()
    {
        Dictionary<ModelId, Share> shares = [];
        foreach (var tx in Transactions)
        {
            var share = shares.GetValueOrDefault(tx.Creator.Id, new Share
            {
                Creator = tx.Creator,
                Player = this,
            });

            share.Count(tx);
            shares[tx.Creator.Id] = share;
        }

        return [.. shares.Values.Where(share => share.Quantity > 0)];
    }

    public Transaction Buy(Creator creator, Quantity amount)
    {
        var value = creator.Value * amount;
        if (Credits < value) throw new ExceedsBalanceException();

        var currentShares = GetShares();
        var currentQuantity = currentShares
            .Where(s => s.Creator.Id == creator.Id)
            .Select(s => s.Quantity.Value)
            .FirstOrDefault(0);

        if (currentQuantity + amount.Value > MaxShares)
            throw new MaxSharesException(MaxShares);

        Credits -= value;

        var tx = Transaction.CreateBuy(this, creator, amount);
        creator.RecordTransaction(tx);
        Transactions.Add(tx);

        return tx;
    }

    public Transaction Sell(Creator creator, Quantity amount)
    {
        var shares = GetShares();
        var quantity = shares
            .Where(s => s.Creator.Id == creator.Id)
            .Select(s => s.Quantity.Value)
            .FirstOrDefault();

        if (quantity < amount.Value)
            throw new ExceedsSharesException();

        var value = creator.Value * amount;
        Credits += value;

        var tx = Transaction.CreateSell(this, creator, amount);
        creator.RecordTransaction(tx);
        Transactions.Add(tx);

        return tx;
    }

    public LootBox AddLootBox()
    {
        LootBox lootBox = LootBox.Create(this);
        LootBoxes.Add(lootBox);

        return lootBox;
    }

    public static Player Create(Name name, Slug slug, TwitchId twitchId, Uri avatarUrl, Credits? credits = null)
    {
        var player = new Player
        {
            Name = name,
            Slug = slug,
            TwitchId = twitchId,
            AvatarUrl = avatarUrl,
            Credits = credits ?? StarterCredits,
        };

        player.LootBoxes.Add(LootBox.Create(player));

        return player;
    }
}