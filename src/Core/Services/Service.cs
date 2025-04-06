using TTX.Core.Repositories;

namespace TTX.Core.Services;


public class Service<T>(IRepository<T> repository) where T : class
{
    protected readonly IRepository<T> repository = repository;
}