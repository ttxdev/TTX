using Microsoft.EntityFrameworkCore;
using TTX.Infrastructure.Data;
using TTX.Models;

namespace TTX.Queries.Players.FindPlayer;

public class FindPlayerHandler(ApplicationDbContext context) : IQueryHandler<FindPlayerQuery, Player?>
{
    public Task<Player?> Handle(FindPlayerQuery request, CancellationToken ct = default)
    {
        return context.Players
            .Include(u => u.Transactions.OrderBy(t => t.CreatedAt))
            .ThenInclude(t => t.Creator)
            .Include(u => u.LootBoxes)
            .ThenInclude(l => l.Result)
            .SingleOrDefaultAsync(e => e.Slug == request.Slug, cancellationToken: ct);
    }
}
