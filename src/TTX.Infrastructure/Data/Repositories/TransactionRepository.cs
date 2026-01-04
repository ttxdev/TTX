using Microsoft.EntityFrameworkCore;
using TTX.App.Repositories;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;

namespace TTX.Infrastructure.Data.Repositories;

public class TransactionRepository(ApplicationDbContext _dbContext) : ITransactionRepository
{
    public Task<Creator?> FindCreator(Slug creatorSlug)
    {
        return _dbContext.Creators.FirstOrDefaultAsync(c => c.Slug == creatorSlug);
    }

    public Task<Player?> FindPlayerWithLootbox(ModelId playerId, ModelId boxId)
    {
        return _dbContext.Players
            .Where(p => p.Id == playerId)
            .Include(p => p.LootBoxes.Where(l => l.Id == boxId))
            .FirstOrDefaultAsync();
    }

    public Task<Player?> FindPlayerWithTransactions(ModelId actorId)
    {
        return _dbContext.Players
            .Include(p => p.Transactions.OrderBy(t => t.CreatedAt))
            .ThenInclude(t => t.Creator)
            .FirstOrDefaultAsync(p => p.Id == actorId);
    }

    public Task<Creator[]> GetCreatorsByMinValue(int minValue)
    {
        return _dbContext.Creators
            .Where(c => c.Value >= minValue)
            .ToArrayAsync();
    }

    public Task SaveChanges()
    {
        return _dbContext.SaveChangesAsync();
    }
}
