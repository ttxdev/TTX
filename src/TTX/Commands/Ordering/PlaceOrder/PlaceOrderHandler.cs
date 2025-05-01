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
                                .Include(p => p.Transactions)
                                .SingleOrDefaultAsync(p => p.Id == request.Actor, ct)
                            ?? throw new PlayerNotFoundException();

            Creator creator = await context.Creators.SingleOrDefaultAsync(c => c.Slug == request.Creator, ct)
                              ?? throw new CreatorNotFoundException();

            Transaction tx = request.IsBuy
                ? player.Buy(creator, request.Amount)
                : player.Sell(creator, request.Amount);

            context.Transactions.Add(tx);
            context.Players.Update(player);
            await context.SaveChangesAsync(ct);
            await mediator.Publish(CreateTransaction.Create(tx), ct);

            return tx;
        }
    }
}
