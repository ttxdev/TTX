using TTX.Core.Models;

namespace TTX.Core.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetDetails(string name);
    Task<Pagination<User>> GetPaginated(
        int page = 1,
        int limit = 10,
        Order[]? order = null,
        Search? search = null
    );
}