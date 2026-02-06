using Microsoft.EntityFrameworkCore;
using TTX.App.Data;
using TTX.App.Dto.LootBoxes;
using TTX.App.Events;
using TTX.App.Events.LootBoxes;
using TTX.App.Events.Transactions;
using TTX.App.Services.LootBoxes.Exceptions;
using TTX.Domain.Exceptions;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;

namespace TTX.App.Services.LootBoxes;

public class LootBoxService(
    ApplicationDbContext _dbContext,
    IEventDispatcher _events,
    Random _random
)
{
    public async Task<Result<LootBoxResultDto>> OpenLootBox(ModelId playerId, ModelId boxId)
    {
        Player? player = await _dbContext.Players
            .Where(p => p.Id == playerId)
            .Include(p => p.LootBoxes.Where(l => l.Id == boxId))
            .FirstOrDefaultAsync();
        if (player is null)
        {
            return Result<LootBoxResultDto>.Err(new NotFoundException<Player>());
        }

        CreatorRarity[] rarities = await GetCreatorRarities();
        WeightedRarity[] weightedRarities = [.. rarities.Select(rarity => new WeightedRarity(rarity))];

        LootBox? lootBox = player.LootBoxes.FirstOrDefault();

        if (lootBox is null)
        {
            return Result<LootBoxResultDto>.Err(new NotFoundException<LootBox>());
        }

        if (lootBox.IsOpen)
        {
            return Result<LootBoxResultDto>.Err(new LootBoxOpenedException());
        }

        int totalWeight = weightedRarities.Sum(wr => wr.Weight);
        int roll = _random.Next(0, totalWeight);

        CreatorRarity? selectedRarity = null;
        foreach (var weightedRarity in weightedRarities)
        {
            if (roll < weightedRarity.Weight)
            {
                selectedRarity = weightedRarity.CreatorRarity;
                break;
            }

            roll -= weightedRarity.Weight;
        }

        OpenLootBoxResult result = new()
        {
            LootBox = lootBox,
            Rarities = rarities,
            Result = selectedRarity!.Value
        };
        lootBox.SetResult(result.Result.Creator);
        Transaction tx = player.Give(lootBox.Result!);

        await _dbContext.SaveChangesAsync();
        await _events.Dispatch(CreateTransactionEvent.Create(tx));
        await _events.Dispatch(OpenLootBoxEvent.Create(result, tx.Id));

        return Result<LootBoxResultDto>.Ok(LootBoxResultDto.Create(result, tx.Id));
    }

    public async Task<CreatorRarity[]> GetCreatorRarities()
    {
        double averageCreatorValue = await _dbContext.Creators.AverageAsync(c => c.Value);
        Creator[] creators = await _dbContext.Creators.Where(creator => creator.Value >= averageCreatorValue).ToArrayAsync();
        double sum = creators.Sum(creator => creator.Value);

        return [.. creators.Select(creator => CreatorRarity.Create(sum, creator))];
    }

    internal record WeightedRarity
    {
        public CreatorRarity CreatorRarity { get; init; }
        public int Weight { get; init; }

        public WeightedRarity(CreatorRarity creatorRarity)
        {
            CreatorRarity = creatorRarity;
            Weight = creatorRarity.Rarity switch
            {
                Rarity.Epic => 5,
                Rarity.Rare => 25,
                Rarity.Common => 50,
                Rarity.Pennies => 100,
                _ => 0
            };
        }
    }
}
