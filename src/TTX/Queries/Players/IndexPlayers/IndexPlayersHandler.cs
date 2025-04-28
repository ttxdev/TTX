using Microsoft.EntityFrameworkCore;
using TTX.Infrastructure.Data;
using TTX.Models;

namespace TTX.Queries.Players.IndexPlayers;

public class IndexPlayersHandler(ApplicationDbContext context) : IQueryHandler<IndexPlayersQuery, Pagination<Player>>
{
    public async Task<Pagination<Player>> Handle(IndexPlayersQuery request, CancellationToken ct = default)
    {
        var query = context.Players.AsQueryable();
        ApplySearch(ref query, request.Search);
        ApplyOrder(ref query, request.Order);

        var total = await query.CountAsync(cancellationToken: ct);
        var data = query.Skip((request.Page - 1) * request.Limit).Take(request.Limit);

        return new Pagination<Player>
        {
            Total = total,
            Data = await data.ToArrayAsync(ct)
        };
    }

    public void ApplyOrder(ref IQueryable<Player> query, Order<PlayerOrderBy>? order)
    {
        Order<PlayerOrderBy> o = order is not null ? order.Value : new Order<PlayerOrderBy>()
        {
            By = PlayerOrderBy.Credits,
            Dir = OrderDirection.Ascending
        };

        query = o.By switch
        {
            PlayerOrderBy.Credits => (o.Dir == OrderDirection.Ascending
                ? query.OrderBy(p => p.Credits)
                : query.OrderByDescending(p => p.Credits)).ThenBy(p => p.Name),
            _ => query.OrderBy(p => p.Name)
        };
    }

    private void ApplySearch(ref IQueryable<Player> query, string? search)
    {
        if (search is null)
            return;

        query = query.Where(p => EF.Functions.ILike(p.Name, $"%{search}%"));
    }
}
