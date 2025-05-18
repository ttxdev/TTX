using Microsoft.EntityFrameworkCore;
using TTX.Dto;
using TTX.Dto.CreatorApplications;
using TTX.Infrastructure.Data;
using TTX.Models;

namespace TTX.Queries.CreatorApplications.IndexCreatorApplications
{
    public class IndexCreatorApplicationHandler(ApplicationDbContext context)
        : IQueryHandler<IndexCreatorApplicationQuery, PaginationDto<CreatorApplicationDto>>
    {
        public async Task<PaginationDto<CreatorApplicationDto>> Handle(IndexCreatorApplicationQuery request,
            CancellationToken ct)
        {
            IQueryable<CreatorApplication> query = context.CreatorApplications
                .Include(c => c.Submitter)
                .AsQueryable();
            ApplySearch(ref query, request.Search);
            ApplyOrder(ref query, request.Order);

            int total = await query.CountAsync(ct);
            CreatorApplication[] apps =
                await query.Skip((request.Page - 1) * request.Limit).Take(request.Limit).ToArrayAsync(ct);

            return new PaginationDto<CreatorApplicationDto>
            {
                Total = total,
                Data = [.. apps.Select(CreatorApplicationDto.Create)]
            };
        }

        private static void ApplySearch(ref IQueryable<CreatorApplication> query, string? search)
        {
            if (search is null)
            {
                return;
            }

            query = query.Where(c => EF.Functions.ILike(c.Name, $"%{search}%"));
        }

        private static void ApplyOrder(ref IQueryable<CreatorApplication> query,
            Order<CreatorApplicationOrderBy>? order)
        {
            Order<CreatorApplicationOrderBy> o = order ?? new Order<CreatorApplicationOrderBy>
            {
                By = CreatorApplicationOrderBy.Name, Dir = OrderDirection.Ascending
            };

            query = o.By switch
            {
                CreatorApplicationOrderBy.Name => (o.Dir == OrderDirection.Ascending
                    ? query.OrderBy(a => a.Name)
                    : query.OrderByDescending(a => a.Name)).ThenBy(c => c.CreatedAt),
                CreatorApplicationOrderBy.Submitter =>
                    (o.Dir == OrderDirection.Ascending
                        ? query.OrderBy(a => a.Submitter.Name)
                        : query.OrderByDescending(a => a.Submitter.Name)).ThenBy(a => a.CreatedAt),
                _ => o.Dir == OrderDirection.Ascending
                    ? query.OrderBy(a => a.CreatedAt)
                    : query.OrderByDescending(a => a.CreatedAt)
            };
        }
    }
}
