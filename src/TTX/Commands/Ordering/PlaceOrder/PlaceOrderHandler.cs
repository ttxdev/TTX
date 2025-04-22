﻿using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Models;

namespace TTX.Commands.Ordering.PlaceOrder;

public class PlaceOrderHandler(ApplicationDbContext context) : ICommandHandler<PlaceOrderCommand, Transaction>
{
    public async Task<Transaction> Handle(PlaceOrderCommand request, CancellationToken ct = default)
    {
        var player = await context.Players
            .Include(p => p.Transactions)
            .SingleOrDefaultAsync(p => p.Slug == request.Actor, cancellationToken: ct)
            ?? throw new PlayerNotFoundException();

        var creator = await context.Creators.SingleOrDefaultAsync(c => c.Slug == request.Creator, ct)
            ?? throw new CreatorNotFoundException();

        var tx = request.IsBuy
              ? player.Buy(creator, request.Amount)
              : player.Sell(creator, request.Amount);

        context.Transactions.Add(tx);
        context.Players.Update(player);
        await context.SaveChangesAsync(ct);

        return tx;
    }
}
