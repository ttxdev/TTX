using TTX.Models;

namespace TTX.ValueObjects
{
    public readonly struct CreatorRarity
    {
        public required Creator Creator { get; init; }
        public required Rarity Rarity { get; init; }

        public static CreatorRarity Create(long sum, Creator creator)
        {
            double calc = creator.Value / (double)sum * 100;
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
}