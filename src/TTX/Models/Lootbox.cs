using System.Collections.Immutable;
using TTX.Exceptions;
using TTX.ValueObjects;

namespace TTX.Models
{
    public class LootBox : Model
    {
        public required ModelId PlayerId { get; init; }
        public ModelId? ResultId { get; private set; }
        public Player Player { get; init; } = null!;
        public Creator? Result { get; private set; }
        public bool IsOpen => ResultId is not null;

        public static LootBox Create(Player player)
        {
            return new LootBox { Player = player, PlayerId = player.Id };
        }

        public OpenLootBoxResult Open(Creator[] creators, Random? random = null)
        {
            if (IsOpen)
            {
                throw new LootBoxOpenedException();
            }

            if (creators.Length == 0)
            {
                throw new InvalidOperationException("No creators provided.");
            }

            long sum = creators.Sum(creator => creator.Value);
            ImmutableArray<CreatorRarity> rarities =
                creators.Select(creator => CreatorRarity.Create(sum, creator)).ToImmutableArray();
            random ??= new Random();
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
            }).ToArray();

            int totalWeight = weightedRarities.Sum(wr => wr.Weight);
            int roll = random.Next(0, totalWeight);

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

            Result = selectedRarity!.Value.Creator;
            ResultId = Result!.Id;

            return new OpenLootBoxResult { LootBox = this, Rarities = rarities, Result = selectedRarity.Value };
        }
    }
}