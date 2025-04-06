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

    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task SaveChanges(CancellationToken cancellationToken = default);
}