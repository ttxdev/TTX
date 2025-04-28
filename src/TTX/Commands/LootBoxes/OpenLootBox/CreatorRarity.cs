using TTX.Models;

namespace TTX.Commands.LootBoxes.OpenLootBox
{
    public class CreatorRarity
    {
        public required Creator Creator { get; set; }
        public required Rarity Rarity { get; set; }

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