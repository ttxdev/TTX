using TTX.Core.Models;

namespace TTX.Core.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetDetails(string name);
    Task<Pagination<User>> GetPaginated(
        int page,
        int limit,
        Order[]? order = null,
        Search? search = null
    );
}