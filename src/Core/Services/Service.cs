using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Core.Services;

public interface IService<T> where T : class
{
    ValueTask<T?> Find(int id);
    Task<T[]> GetAllByIds(int[] ids);
    Task<T[]> GetAll();

    Task<Pagination<T>> GetPaginated(
      int page = 1,
      int limit = 10,
      Order[]? order = null,
      Search? search = null
    );
}

public class Service<T>(IRepository<T> repository) : IService<T>
  where T : class
{
    protected readonly IRepository<T> repository = repository;

    public ValueTask<T?> Find(int id) => repository.Find(id);
    public Task<T[]> GetAll() => repository.GetAll();

    public Task<T[]> GetAllByIds(int[] ids) => repository.GetAllByIds(ids);

    public Task<Pagination<T>> GetPaginated(
      int page = 1,
      int limit = 10,
      Order[]? order = null,
      Search? search = null
    ) => repository.GetPaginated(page, limit, order, search);
}