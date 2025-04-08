using TTX.Core.Models;

namespace TTX.Core.Repositories;

public interface ICreatorRepository : IRepository<Creator>
{
    public Task<long> GetValueSum();
    public Task<Creator[]> GetAllAbove(long value);
    public Task<Creator[]> GetAll();
    public Task<Creator[]> GetAllByIds(int[] ids);
    public Task<Creator?> GetDetails(string slug);
    public Task<int?> GetId(string slug);
    public Task<Creator?> UpdateStreamInfo(int id, StreamStatus status);
    Task<Pagination<Creator>> GetPaginated(
        int page = 1,
        int limit = 10,
        Order[]? order = null,
        Search? search = null
    );
}