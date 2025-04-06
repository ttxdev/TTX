namespace TTX.Core.Models;

public class LootBoxResult
{
    public required CreatorRarity Result { get; set; }
    public required CreatorRarity[] Rarities { get; set; }
}

public class LootBox : ModelBase
{
    public required int UserId { get; set; }
    public int? ResultId { get; set; } = null;
    public required User User { get; set; }
    public Creator? Result { get; set; }
    public bool IsOpen { get => Result != null; }

    public static LootBox Create(User user)
    {
        return new LootBox()
        {
            User = user,
            UserId = user.Id,
        };
    }

    public LootBoxResult Open(Creator[] creators)
    {
        var rarities = GetRarities(creators);
        var random = new Random();
        var weightedRarities = rarities.Select(rarity => new
        {
            Rarity = rarity,
            Weight = rarity.Rarity switch
            {
                Rarity.Epic => 5,
                Rarity.Rare => 25,
                Rarity.Common => 50,
                Rarity.Pennies => 100,
                _ => 0
            }
        }).ToList();

        if (Result is not null)
            return new LootBoxResult()
            {
                Rarities = rarities,
                Result = new CreatorRarity
                {
                    Creator = Result,
                    Rarity = rarities
                  .Where(r => r.Creator.Id == Result!.Id)
                  .Select(r => r.Rarity)
                  .FirstOrDefault(Rarity.Pennies)
                }
            };


        var totalWeight = weightedRarities.Sum(wr => wr.Weight);

        var roll = random.Next(0, totalWeight);
        CreatorRarity? selectedRarity = null;
        foreach (var weightedRarity in weightedRarities)
        {
            if (roll < weightedRarity.Weight)
            {
                selectedRarity = weightedRarity.Rarity;
                break;
            }
            roll -= weightedRarity.Weight;
        }

        Result = selectedRarity!.Creator;

        return new LootBoxResult()
        {
            Rarities = rarities,
            Result = selectedRarity
        };
    }

    public static CreatorRarity[] GetRarities(Creator[] creators)
    {
        var sum = creators.Sum(creator => creator.Value);
        return [.. creators.Select(creator => CreatorRarity.Create(sum, creator))];
    }
}