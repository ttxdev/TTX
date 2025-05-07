using Microsoft.EntityFrameworkCore;
using TTX.Dto;
using TTX.Dto.Players;
using TTX.Infrastructure.Data;
using TTX.Models;

namespace TTX.Queries.Players.IndexPlayers
{
    public class IndexPlayersHandler(ApplicationDbContext c)
        : PlayerQueryHandler(c), IQueryHandler<IndexPlayersQuery, PaginationDto<PlayerDto>>
    {
        public async Task<PaginationDto<PlayerDto>> Handle(IndexPlayersQuery request, CancellationToken ct = default)
        {
            IQueryable<Player> query = Context.Players.AsQueryable();
            ApplySearch(ref query, request.Search);
            ApplyOrder(ref query, request.Order);

            int total = await query.CountAsync(ct);
            IQueryable<Player> data = query.Skip((request.Page - 1) * request.Limit).Take(request.Limit);
            Player[] players = await data.ToArrayAsync(ct);
            Dictionary<int, PortfolioSnapshot[]> history = await GetHistoryFor([..players], request.HistoryParams.Step,
                request.HistoryParams.After, ct);

            return new PaginationDto<PlayerDto>
            {
                Total = total,
                Data =
                [
                    ..players.Select(p =>
                    {
                        if (history.TryGetValue(p.Id, out PortfolioSnapshot[]? snap))
                        {
                            p.History = [.. snap];
                        }

                        return PlayerDto.Create(p);
                    })
                ]
            };
        }

        private static void ApplyOrder(ref IQueryable<Player> query, Order<PlayerOrderBy>? order)
        {
            Order<PlayerOrderBy> o = order ?? new Order<PlayerOrderBy>
            {
                By = PlayerOrderBy.Credits, Dir = OrderDirection.Ascending
            };

            query = o.By switch
            {
                PlayerOrderBy.Credits => (o.Dir == OrderDirection.Ascending
                    ? query.OrderBy(p => p.Credits)
                    : query.OrderByDescending(p => p.Credits)).ThenBy(p => p.Name),
                PlayerOrderBy.Portfolio => (o.Dir == OrderDirection.Ascending
                    ? query.OrderBy(p => p.Portfolio)
                    : query.OrderByDescending(p => p.Portfolio)).ThenBy(p => p.Name),
                _ => query.OrderBy(p => p.Name)
            };
        }

        private static void ApplySearch(ref IQueryable<Player> query, string? search)
        {
            if (search is null)
            {
                return;
            }

            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{search}%"));
        }
    }
}