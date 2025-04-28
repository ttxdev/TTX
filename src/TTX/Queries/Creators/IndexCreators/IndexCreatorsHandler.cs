using Microsoft.EntityFrameworkCore;
using TTX.Infrastructure.Data;
using TTX.Models;

namespace TTX.Queries.Creators.IndexCreators;

public class IndexCreatorsHandler(ApplicationDbContext context) : CreatorQueryHandler(context), IQueryHandler<IndexCreatorsQuery, Pagination<Creator>>
{
    public async Task<Pagination<Creator>> Handle(IndexCreatorsQuery request, CancellationToken ct = default)
    {
        var query = Context.Creators.AsQueryable();
        ApplySearch(ref query, request.Search);
        ApplyOrder(ref query, request.Order);

        var total = await query.CountAsync(ct);
        var creators = await query.Skip((request.Page - 1) * request.Limit).Take(request.Limit).ToArrayAsync(cancellationToken: ct);
        var history = await GetHistoryFor([.. creators], request.HistoryParams.Step, request.HistoryParams.After, ct);

        return new Pagination<Creator>
        {
            Total = total,
            Data = [.. creators.Select(c =>
            {
                if (history.TryGetValue(c.Id, out Vote[]? value))
                    c.History = [.. value];

                return c;
            })],
        };
    }

    private void ApplySearch(ref IQueryable<Creator> query, string? search)
    {
        if (search is null)
            return;

        query = query.Where(c => EF.Functions.ILike(c.Name, $"%{search}%"));
    }

    private void ApplyOrder(ref IQueryable<Creator> query, Order<CreatorOrderBy>? order)
    {
        Order<CreatorOrderBy> o = order is not null
            ? order.Value
            : new Order<CreatorOrderBy>()
            {
                By = CreatorOrderBy.Name,
                Dir = OrderDirection.Ascending
            };

        query = o.By switch
        {
            CreatorOrderBy.Value => (o.Dir == OrderDirection.Ascending
                ? query.OrderBy(c => c.Value)
                : query.OrderByDescending(c => c.Value)).ThenBy(c => c.Name),
            CreatorOrderBy.IsLive =>
                (o.Dir == OrderDirection.Ascending
                    ? query.OrderBy(c => c.StreamStatus.IsLive)
                    : query.OrderByDescending(c => c.StreamStatus.IsLive)).ThenBy(c => c.Name),
            _ => o.Dir == OrderDirection.Ascending
                ? query.OrderBy(c => c.Name)
                : query.OrderByDescending(c => c.Name)
        };
    }
}
