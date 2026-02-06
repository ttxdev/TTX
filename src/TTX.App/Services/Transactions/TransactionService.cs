using TTX.App.Dto.LootBoxes;
using TTX.App.Events;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;
using TTX.Domain.Exceptions;
using TTX.App.Events.Transactions;
using TTX.App.Events.LootBoxes;
using TTX.App.Services.Transactions.Exceptions;
using TTX.App.Data;
using Microsoft.EntityFrameworkCore;

namespace TTX.App.Services.Transactions;

public class TransactionService(
    ApplicationDbContext _dbContext,
    IEventDispatcher _events,
    Random _random
)
{
    public async Task<Result<ModelId>> PlaceOrder(ModelId actorId, Slug creatorSlug, TransactionAction action, Quantity quantity)
    {
        Player? player = await _dbContext.Players
                    .Include(p => p.Transactions.OrderBy(t => t.CreatedAt))
                    .ThenInclude(t => t.Creator)
                    .FirstOrDefaultAsync(p => p.Id == actorId);
        if (player is null)
        {
            return Result<ModelId>.Err(new NotFoundException<Player>());
        }

        Creator? creator = await _dbContext.Creators.FirstOrDefaultAsync(c => c.Slug == creatorSlug);

        if (creator is null)
        {
            return Result<ModelId>.Err(new NotFoundException<Creator>());
        }

        if (!creator.StreamStatus.IsLive)
        {
            return Result<ModelId>.Err(new MarketClosedException());
        }

        Transaction tx = action switch
        {
            TransactionAction.Buy => player.Buy(creator, quantity),
            TransactionAction.Sell => player.Sell(creator, quantity),
            _ => throw new InvalidActionException("Invalid transaction action")
        };

        await _dbContext.SaveChangesAsync();
        await _events.Dispatch(CreateTransactionEvent.Create(tx));

        return Result<ModelId>.Ok(tx.Id);
    }

    public async Task<CreatorRarity[]> GetCreatorRarities()
    {
        double averageCreatorValue = await _dbContext.Creators.AverageAsync(c => c.Value);
        Creator[] creators = await _dbContext.Creators.Where(creator => creator.Value >= averageCreatorValue).ToArrayAsync();
        double sum = creators.Sum(creator => creator.Value);

        return [.. creators.Select(creator => CreatorRarity.Create(sum, creator))];
    }

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
