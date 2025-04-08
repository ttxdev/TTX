using System.ComponentModel.DataAnnotations;
using TTX.Core.Exceptions;

namespace TTX.Core.Models;

public enum UserType
{
    User,
    Admin
}

public class User : ModelBase, IValidatableObject
{
    public const int MAX_SHARES = 1_000;
    public const int MIN_CREDITS = 0;
    [MinLength(4)]
    [MaxLength(25)]
    public required string Name { get; set; }
    public required string TwitchId { get; set; }
    public required string AvatarUrl { get; set; }
    [Range(MIN_CREDITS, long.MaxValue)]
    public long Credits { get; private set; } = 100;
    public UserType Type { get; set; } = UserType.User;
    public ICollection<Transaction> Transactions { get; set; } = [];
    public ICollection<LootBox> LootBoxes { get; set; } = [];

    public Share[] GetShares()
    {
        var shares = new Dictionary<int, Share>();
        foreach (var tx in Transactions)
        {
            var share = shares.GetValueOrDefault(tx.CreatorId, new Share
            {
                Creator = tx.Creator,
                User = this,
                Quantity = 0
            });
            if (tx.IsBuy()) share.Quantity += tx.Quantity;
            else share.Quantity -= tx.Quantity;
            shares[tx.CreatorId] = share;
        }

        return [.. shares.Values.Where(share => share.Quantity > 100)];
    }

    public Transaction Buy(Creator creator, int amount)
    {

        if (amount <= 0) throw new ZeroAmountException();

        var value = creator.Value * amount;
        if (Credits < value) throw new ExceedsBalanceException();

        var currentShares = GetShares();
        var currentQuantity = currentShares
            .Where(s => s.Creator.Id == creator.Id)
            .Select(s => s.Quantity)
            .FirstOrDefault(0);

        if (currentQuantity + amount > MAX_SHARES)
            throw new MaxSharesException(MAX_SHARES);

        Credits -= value;

        var tx = Transaction.CreateBuy(this, creator, amount);
        Transactions.Add(tx);

        return tx;
    }

    public Transaction Sell(Creator creator, int amount)
    {
        var shares = GetShares();
        var quantity = shares
            .Where(s => s.Creator.Id == creator.Id)
            .Select(s => s.Quantity)
            .FirstOrDefault();

        if (quantity < amount)
            throw new ExceedsSharesException();

        var value = creator.Value * amount;
        Credits += value;
        var tx = Transaction.CreateSell(this, creator, amount);
        Transactions.Add(tx);

        return tx;
    }

    public LootBoxResult Gamba(Creator[] creators)
    {
        var lootBox = LootBoxes.LastOrDefault() ?? throw new NoLootBoxesException();
        LootBoxes.Remove(lootBox);

        return lootBox.Open(creators);
    }

    public bool IsAdmin() => Type == UserType.Admin;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Credits < MIN_CREDITS)
            yield return new ValidationResult($"Credits must be greater than or equal to {MIN_CREDITS}.", [nameof(Credits)]);
    }

    public static User Create(TwitchUser tUser)
    {
        var user = new User
        {
            TwitchId = tUser.Id,
            AvatarUrl = tUser.AvatarUrl,
            Name = tUser.DisplayName,
            Credits = 100
        };
        user.LootBoxes.Add(LootBox.Create(user));

        var validatorResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(user);
        if (!Validator.TryValidateObject(user, validationContext, validatorResults, true))
            throw new ModelValidationException(validatorResults);

        return user;
    }
}