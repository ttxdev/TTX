using TTX.Core.Models;

namespace TTX.Core.Repositories;

public interface ICreatorRepository : IRepository<Creator>
{
    public Task<long> GetValueSum();
    public Task<Creator[]> GetAllAbove(long value);
    public Task<Creator?> FindBySlug(string slug);
}