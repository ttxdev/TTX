using Microsoft.EntityFrameworkCore;
using TTX.Infrastructure.Data;
using TTX.Models;

namespace TTX.Queries.Creators.FindCreator
{
    public class FindCreatorHandler(ApplicationDbContext context)
        : CreatorQueryHandler(context), IQueryHandler<FindCreatorQuery, Creator?>
    {
        public async Task<Creator?> Handle(FindCreatorQuery request, CancellationToken ct)
        {
            Creator? creator = await Context.Creators
                .Include(c => c.Transactions.OrderBy(t => t.CreatedAt))
                .ThenInclude(t => t.Player)
                .FirstOrDefaultAsync(c => c.Slug == request.Slug, ct);

            if (creator is null)
            {
                return null;
            }

            Dictionary<int, Vote[]> history =
                await GetHistoryFor([creator], request.HistoryParams.Step, request.HistoryParams.After, ct);
            if (history.TryGetValue(creator.Id, out Vote[]? value))
            {
                creator.History = [.. value];
            }

            return creator;
        }
    }
}