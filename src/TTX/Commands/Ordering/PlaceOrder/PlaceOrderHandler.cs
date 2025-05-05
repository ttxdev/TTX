using MediatR;
using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Models;
using TTX.Notifications.Transactions;

namespace TTX.Commands.Ordering.PlaceOrder
{
    public class PlaceOrderHandler(ApplicationDbContext context, IMediator mediator)
        : ICommandHandler<PlaceOrderCommand, Transaction>
    {
        public async Task<Transaction> Handle(PlaceOrderCommand request, CancellationToken ct = default)
        {
            Player player = await context.Players
                                .Include(p => p.Transactions.OrderBy(t => t.CreatedAt))
                                .ThenInclude(t => t.Creator)
                                .SingleOrDefaultAsync(p => p.Id == request.ActorId, ct)
                            ?? throw new NotFoundException<Player>();

            Creator creator = await context.Creators.SingleOrDefaultAsync(c => c.Slug == request.Creator, ct)
                              ?? throw new NotFoundException<Creator>();

            Transaction tx = request.Action switch
            {
                TransactionAction.Buy => player.Buy(creator, request.Amount),
                TransactionAction.Sell => player.Sell(creator, request.Amount),
                _ => throw new InvalidActionException("Invalid transaction action")
            };

            context.Transactions.Add(tx);
            context.Players.Update(player);
            await context.SaveChangesAsync(ct);
            await mediator.Publish(CreateTransaction.Create(tx), ct);

            return tx;
        }
    }
}