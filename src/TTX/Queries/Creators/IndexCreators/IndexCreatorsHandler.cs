using Microsoft.EntityFrameworkCore;
using TTX.Dto;
using TTX.Dto.Creators;
using TTX.Infrastructure.Data;
using TTX.Models;

namespace TTX.Queries.Creators.IndexCreators
{
    public class IndexCreatorsHandler(ApplicationDbContext context)
        : CreatorQueryHandler(context), IQueryHandler<IndexCreatorsQuery, PaginationDto<CreatorDto>>
    {
        public async Task<PaginationDto<CreatorDto>> Handle(IndexCreatorsQuery request, CancellationToken ct = default)
        {
            IQueryable<Creator> query = Context.Creators.AsQueryable();
            ApplySearch(ref query, request.Search);
            ApplyOrder(ref query, request.Order);

            int total = await query.CountAsync(ct);
            Creator[] creators =
                await query.Skip((request.Page - 1) * request.Limit).Take(request.Limit).ToArrayAsync(ct);
            Dictionary<int, Vote[]> history = await GetHistoryFor([.. creators], request.HistoryParams.Step,
                request.HistoryParams.After, ct);

            return new PaginationDto<CreatorDto>
            {
                Total = total,
                Data =
                [
                    .. creators.Select(c =>
                    {
                        if (history.TryGetValue(c.Id, out Vote[]? value))
                        {
                            c.History = [.. value];
                        }

                        return CreatorDto.Create(c);
                    })
                ]
            };
        }

        private static void ApplySearch(ref IQueryable<Creator> query, string? search)
        {
            if (search is null)
            {
                return;
            }

            query = query.Where(c => EF.Functions.ILike(c.Name, $"%{search}%"));
        }

        private static void ApplyOrder(ref IQueryable<Creator> query, Order<CreatorOrderBy>? order)
        {
            Order<CreatorOrderBy> o = order ?? new Order<CreatorOrderBy>
            {
                By = CreatorOrderBy.Name, Dir = OrderDirection.Ascending
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
}