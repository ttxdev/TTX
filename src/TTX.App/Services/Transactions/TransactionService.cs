using TTX.App.Events;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;
using TTX.Domain.Exceptions;
using TTX.App.Events.Transactions;
using TTX.App.Data;
using Microsoft.EntityFrameworkCore;
using TTX.App.Services.Transactions.Exceptions;

namespace TTX.App.Services.Transactions;

public class TransactionService(
    ApplicationDbContext _dbContext,
    IEventDispatcher _events
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
}
