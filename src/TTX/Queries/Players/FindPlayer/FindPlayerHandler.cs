using Microsoft.EntityFrameworkCore;
using TTX.Dto.Players;
using TTX.Infrastructure.Data;
using TTX.Models;

namespace TTX.Queries.Players.FindPlayer
{
    public class FindPlayerHandler(ApplicationDbContext context)
        : PlayerQueryHandler(context), IQueryHandler<FindPlayerQuery, PlayerDto?>
    {
        public async Task<PlayerDto?> Handle(FindPlayerQuery request, CancellationToken ct = default)
        {
            Player? player = await Context.Players
                .Include(u => u.Transactions.OrderBy(t => t.CreatedAt))
                .ThenInclude(t => t.Creator)
                .Include(u => u.LootBoxes)
                .ThenInclude(l => l.Result)
                .SingleOrDefaultAsync(e => e.Slug == request.Slug, ct);

            if (player is null)
            {
                return null;
            }

            Dictionary<int, PortfolioSnapshot[]> history =
                await GetHistoryFor([player], request.HistoryParams.Step, request.HistoryParams.After, ct);
            if (history.TryGetValue(player.Id, out PortfolioSnapshot[]? portfolio))
            {
                player.History = [.. portfolio];
            }

            return PlayerDto.Create(player);
        }
    }
}