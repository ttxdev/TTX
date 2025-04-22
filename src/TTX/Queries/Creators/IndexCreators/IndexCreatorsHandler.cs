using Microsoft.EntityFrameworkCore;
using TTX.Infrastructure.Data;
using TTX.Models;

namespace TTX.Queries.Creators.IndexCreators;

public class IndexCreatorsHandler(ApplicationDbContext context) : CreatorQueryHandler(context), IQueryHandler<IndexCreatorsQuery, Pagination<Creator>>
{
    public async Task<Pagination<Creator>> Handle(IndexCreatorsQuery request, CancellationToken ct = default)
    {
        var query = Context.Creators.AsQueryable();
        if (request.Search is not null)
            query = query.Where(e => EF.Property<string>(e, request.Search.Value.By)!.ToLower().Contains(request.Search.Value.Value.ToLower()));

        if (request.Order.Length > 0)
        {
            // NOTE(dylhack): This is a hack to get around the fact that EF Core doesn't support
            //                ordering by a nested property. We should probably fix this in the future.
            IOrderedQueryable<Creator> orderedQuery = request.Order[0].By == "IsLive"
                    ? (request.Order[0].Dir == OrderDirection.Ascending ? query.OrderBy(e => e.StreamStatus.IsLive) : query.OrderByDescending(e => e.StreamStatus.IsLive))
                    : (request.Order[0].Dir == OrderDirection.Ascending ? query.OrderBy(e => EF.Property<object>(e, request.Order[0].By!)) : query.OrderByDescending(e => EF.Property<object>(e, request.Order[0].By!)));

            foreach (Order o in request.Order.Skip(1))
            {
                orderedQuery = o.By == "IsLive"
                    ? (o.Dir == OrderDirection.Ascending ? orderedQuery.ThenBy(e => e.StreamStatus.IsLive) : orderedQuery.ThenByDescending(e => e.StreamStatus.IsLive))
                    : (o.Dir == OrderDirection.Ascending ? orderedQuery.ThenBy(e => EF.Property<object>(e, o.By!)) : orderedQuery.ThenByDescending(e => EF.Property<object>(e, o.By!)));
            }
            query = orderedQuery;
        }

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
}
