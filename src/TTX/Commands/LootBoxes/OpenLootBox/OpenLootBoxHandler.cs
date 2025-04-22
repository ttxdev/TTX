using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using TTX.Infrastructure.Data;

namespace TTX.Commands.LootBoxes.OpenLootBox;

public class OpenLootBoxHandler(
    ApplicationDbContext context,
    Random? random = null
) : ICommandHandler<OpenLootBoxCommand, OpenLootBoxResult>
{
    public const int MinValue = 100;
    public readonly Random Random = random ?? new Random();

    public async Task<OpenLootBoxResult> Handle(OpenLootBoxCommand request, CancellationToken ct = default)
    {
        var rarities = await GetRarities();
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


        var totalWeight = weightedRarities.Sum(wr => wr.Weight);

        var roll = Random.Next(0, totalWeight);
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

        return new OpenLootBoxResult()
        {
            Rarities = rarities,
            Result = selectedRarity!
        };
    }

    private async Task<ImmutableArray<CreatorRarity>> GetRarities()
    {
        var creators = await context.Creators.Where(c => c.Value >= MinValue).ToArrayAsync();

        var sum = creators.Sum(creator => creator.Value);
        return [.. creators.Select(creator => CreatorRarity.Create(sum, creator))];
    }
}

