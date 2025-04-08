using Microsoft.EntityFrameworkCore;
using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Infrastructure.Data.Repositories;

public class CreatorRepository(ApplicationDbContext context) : RepositoryBase<Creator>(context), ICreatorRepository
{
    public Task<Creator[]> GetAll() => DbSet.ToArrayAsync();
    public Task<Creator[]> GetAllByIds(int[] ids) => DbSet.Where(c => ids.Contains(c.Id)).ToArrayAsync();
    public Task<Creator?> GetDetails(string slug) => DbSet
            .Include(c => c.Transactions.OrderByDescending(t => t.CreatedAt))
            .ThenInclude(t => t.User)
            .FirstOrDefaultAsync(c => c.Slug == slug);
    public Task<Creator[]> GetAllAbove(long value) => DbSet.Where(c => c.Value > value).ToArrayAsync();
    public Task<long> GetValueSum() => DbSet.SumAsync(c => c.Value);
    public async Task<Creator?> UpdateStreamInfo(int id, StreamStatus status)
    {
        var creator = DbSet.FirstOrDefault(c => c.Id == id);
        if (creator is null) return null;

        creator.StreamStatus = status;
        Update(creator);
        await SaveChanges();

        return creator;
    }

    public Task<Creator?> Find(int creatorId) => DbSet.FirstOrDefaultAsync(c => c.Id == creatorId);

    public override async Task<Pagination<Creator>> GetPaginated(int page = 1, int limit = 10, Order[]? order = null, Search? search = null)
    {
        var query = DbSet.AsQueryable();

        if (search is not null)
            query = query.Where(e => EF.Property<string>(e, search.By)!.ToLower().Contains(search.Value.ToLower()));

        if (order?.Length > 0)
        {
            // NOTE(dylhack): This is a hack to get around the fact that EF Core doesn't support
            //                ordering by a nested property. We should probably fix this in the future.
            IOrderedQueryable<Creator> orderedQuery = order[0].By == "IsLive"
                    ? (order[0].Dir == OrderDirection.Ascending ? query.OrderBy(e => e.StreamStatus.IsLive) : query.OrderByDescending(e => e.StreamStatus.IsLive))
                    : (order[0].Dir == OrderDirection.Ascending ? query.OrderBy(e => EF.Property<object>(e, order[0].By!)) : query.OrderByDescending(e => EF.Property<object>(e, order[0].By!)));

            for (int i = 1; i < order.Length; i++)
            {
                var o = order[i];
                orderedQuery = o.By == "IsLive"
                    ? (o.Dir == OrderDirection.Ascending ? orderedQuery.ThenBy(e => e.StreamStatus.IsLive) : orderedQuery.ThenByDescending(e => e.StreamStatus.IsLive))
                    : (o.Dir == OrderDirection.Ascending ? orderedQuery.ThenBy(e => EF.Property<object>(e, o.By!)) : orderedQuery.ThenByDescending(e => EF.Property<object>(e, o.By!)));
            }

            query = orderedQuery;
        }

        var total = await query.CountAsync();
        var data = query.Skip((page - 1) * limit).Take(limit);

        Console.WriteLine(data.ToQueryString());
        return new Pagination<Creator>
        {
            Total = total,
            Data = await data.ToArrayAsync()
        };
    }

    public async Task<int?> GetId(string slug)
    {
        return await DbSet.Where(c => c.Slug == slug).Select(c => c.Id).FirstOrDefaultAsync();
    }

  public Task<Creator?> FindBySlug(string slug)
  {
    return DbSet.FirstOrDefaultAsync(c => c.Slug == slug);
  }
}
