using TTX.Domain.Models;

namespace TTX.Domain.ValueObjects;

public readonly struct CreatorRarity
{
    public required Creator Creator { get; init; }
    public required Rarity Rarity { get; init; }

    public static CreatorRarity Create(double sum, Creator creator)
    {
        double calc = creator.Value / sum * 100.0;
        Rarity rarity = calc switch
        {
            >= 0 and < 1 => Rarity.Pennies,
            >= 1 and < 5 => Rarity.Common,
            >= 5 and < 20 => Rarity.Rare,
            _ => Rarity.Epic
        };

        return new CreatorRarity { Creator = creator, Rarity = rarity };
    }
}
