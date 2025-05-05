using System.Collections.Immutable;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TTX.Exceptions;
using TTX.Infrastructure.Data;
using TTX.Models;
using TTX.Notifications.Players;

namespace TTX.Commands.Players.CalculatePortfolio
{
    public class CalculatePortfolioHandler(ApplicationDbContext context, IMediator mediator)
        : ICommandHandler<CalculatePortfolioCommand, PortfolioSnapshot>
    {
        public async Task<PortfolioSnapshot> Handle(CalculatePortfolioCommand request, CancellationToken ct)
        {
            PortfolioSnapshot? last = await context.Portfolios.Where(p => p.PlayerId == request.PlayerId)
                .OrderByDescending(p => p.Time)
                .FirstOrDefaultAsync(ct);

            Player player = await context.Players
                                .Include(p => p.Transactions.OrderBy(t => t.CreatedAt))
                                .ThenInclude(t => t.Creator)
                                .SingleOrDefaultAsync(p => p.Id == request.PlayerId, ct) ??
                            throw new NotFoundException<Player>();

            PortfolioSnapshot snapshot = CalculatePortfolio(player);

            await context.Database.ExecuteSqlAsync(
                $"INSERT INTO player_portfolios (player_id, value, time) VALUES ({snapshot.PlayerId.Value}, {snapshot.Value}, {snapshot.Time})",
                ct);
            context.Players.Update(player);
            await context.SaveChangesAsync(ct);
            await mediator.Publish(UpdatePlayerPortfolio.Create(snapshot), ct);

            return snapshot;
        }

        private static PortfolioSnapshot CalculatePortfolio(Player player)
        {
            ImmutableArray<Share> shares = player.GetShares();
            long portfolio = shares
                .Aggregate(0L,
                    (acc, share) => acc + (share.Creator.Value * share.Quantity.Value));

            return player.RecordPortfolio(portfolio);
        }
    }
}