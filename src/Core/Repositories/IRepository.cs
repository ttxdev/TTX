using TTX.Core.Models;

namespace TTX.Core.Repositories;

public class Search
{
    public required string By { get; set; }
    public required string Value { get; set; }
}

public enum OrderDirection
{
    Ascending,
    Descending
}

public class Order()
{
    public required string By { get; set; }
    public required OrderDirection Dir { get; set; }
}


public interface IRepository<T> where T : class
{
    ValueTask<T?> Find(int id);
    Task<T[]> GetAll();
    Task<Pagination<T>> GetPaginated(
        int page = 1,
        int limit = 10,
        Order[]? order = null,
        Search? search = null
    );
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task SaveChanges(CancellationToken cancellationToken = default);
    Task<T[]> GetAllByIds(int[] ids);
}