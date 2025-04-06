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
    public virtual ValueTask<T?> Find(int id) => DbSet.FindAsync(id);
    public virtual Task<T[]> GetAll() => DbSet.ToArrayAsync();
    public virtual Task<T[]> GetAllByIds(int[] ids) => DbSet.Where(e => ids.Contains(e.Id)).ToArrayAsync();
    public Task SaveChanges(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Pagination<T>> GetPaginated(int page = 1, int limit = 10, Order[]? order = null, Search? search = null)
    {
        var query = DbSet.AsQueryable();

        if (search is not null)
            query = query.Where(e => EF.Property<string>(e, search.By)!.ToLower().Contains(search.Value.ToLower()));

        if (order is not null)
            foreach (var o in order)
                query = o.Dir == OrderDirection.Ascending
                    ? query.OrderBy(e => EF.Property<object>(e, o.By!))
                    : query.OrderByDescending(e => EF.Property<object>(e, o.By!));

        var total = await query.CountAsync();
        var data = query.Skip((page - 1) * limit).Take(limit);

        return new Pagination<T>
        {
            Total = total,
            Data = await data.ToArrayAsync()
        };
    }

}