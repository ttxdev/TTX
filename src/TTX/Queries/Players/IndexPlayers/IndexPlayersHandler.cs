using Microsoft.EntityFrameworkCore;
using TTX.Infrastructure.Data;
using TTX.Models;

namespace TTX.Queries.Players.IndexPlayers;

public class IndexPlayersHandler(ApplicationDbContext context) : IQueryHandler<IndexPlayersQuery, Pagination<Player>>
{
    public async Task<Pagination<Player>> Handle(IndexPlayersQuery request, CancellationToken ct = default)
    {
        var query = context.Players.AsQueryable();
        if (request.Search is not null)
            query = query.Where(e => EF.Property<string>(e, request.Search.Value.By)!.ToLower().Contains(request.Search.Value.Value.ToLower()));

        if (request.Order.Length > 0)
        {
            var orderedQuery = request.Order[0].Dir == OrderDirection.Ascending
                ? query.OrderBy(e => EF.Property<object>(e, request.Order[0].By!))
                : query.OrderByDescending(e => EF.Property<object>(e, request.Order[0].By!));

            foreach (Order o in request.Order.Skip(1))
            {
                orderedQuery = o.Dir == OrderDirection.Ascending
                    ? orderedQuery.ThenBy(e => EF.Property<object>(e, o.By!))
                    : orderedQuery.ThenByDescending(e => EF.Property<object>(e, o.By!));
            }

            query = orderedQuery;
        }

        var total = await query.CountAsync(cancellationToken: ct);
        var data = query.Skip((request.Page - 1) * request.Limit).Take(request.Limit);

        return new Pagination<Player>
        {
            Total = total,
            Data = await data.ToArrayAsync(ct)
        };
    }
}
