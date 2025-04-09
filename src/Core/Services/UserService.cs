using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Core.Services;

public interface IUserService
{
    Task<User?> GetDetails(string username);
    Task<Pagination<User>> GetPaginated(
        int page = 1,
        int limit = 10,
        Order[]? order = null,
        Search? search = null
    );
    Task<User> Onboard(TwitchUser tUser);
}

public class UserService(IUserRepository repository) : IUserService
{
    public Task<User?> GetDetails(string username) => repository.GetDetails(username);

    public Task<Pagination<User>> GetPaginated(int page = 1, int limit = 10, Order[]? order = null, Search? search = null) => repository.GetPaginated(page, limit, order, search);

    public async Task<User> Onboard(TwitchUser tUser)
    {
        var user = User.Create(tUser);
        repository.Add(user);
        await repository.SaveChanges();

        return user;
    }
}