using Microsoft.EntityFrameworkCore;
using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Infrastructure.Data.Repositories;

public class RepositoryBase<T>(ApplicationDbContext context) : IRepository<T> where T : ModelBase
{
    protected readonly DbSet<T> DbSet = context.Set<T>();
    public void Add(T entity) => DbSet.Add(entity);
    public void Update(T entity) => DbSet.Update(entity);
    public void Remove(T entity) => DbSet.Remove(entity);
    public Task SaveChanges(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
    public virtual async Task<Pagination<T>> GetPaginated(int page = 1, int limit = 10, Order[]? order = null, Search? search = null)
    {
        var query = DbSet.AsQueryable();

        if (search is not null)
            query = query.Where(e => EF.Property<string>(e, search.By)!.ToLower().Contains(search.Value.ToLower()));

        if (order is not null && order.Length > 0)
        {
            IOrderedQueryable<T> orderedQuery = order[0].Dir == OrderDirection.Ascending 
                ? query.OrderBy(e => EF.Property<object>(e, order[0].By!)) 
                : query.OrderByDescending(e => EF.Property<object>(e, order[0].By!));

            for (int i = 1; i < order.Length; i++)
            {
                var o = order[i];
                orderedQuery = o.Dir == OrderDirection.Ascending 
                    ? orderedQuery.ThenBy(e => EF.Property<object>(e, o.By!)) 
                    : orderedQuery.ThenByDescending(e => EF.Property<object>(e, o.By!));
            }

            query = orderedQuery;
        }

        var total = await query.CountAsync();
        var data = query.Skip((page - 1) * limit).Take(limit);

        return new Pagination<T>
        {
            Total = total,
            Data = await data.ToArrayAsync()
        };
    }
}