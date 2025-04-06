using TTX.Core.Models;

namespace TTX.Core.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> FindByTwitchId(string twitchId);
    Task<User?> FindByName(string name);
}