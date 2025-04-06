namespace TTX.Core.Models;

public enum Rarity
{
    Pennies,
    Common,
    Rare,
    Epic,
}

public class CreatorRarity
{
    public required Creator Creator { get; set; }
    public required Rarity Rarity { get; set; }

    public static CreatorRarity Create(long sum, Creator creator)
    {
        var calc = creator.Value / (double)sum * 100;
        Rarity rarity;
        if (calc >= 0 && calc < 1)
            rarity = Rarity.Pennies;
        else if (calc >= 1 && calc < 5)
            rarity = Rarity.Common;
        else if (calc >= 5 && calc < 20)
            rarity = Rarity.Rare;
        else
            rarity = Rarity.Epic;

        return new CreatorRarity
        {
            Creator = creator,
            Rarity = rarity
        };
    }
}