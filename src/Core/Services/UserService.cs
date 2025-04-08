using TTX.Core.Exceptions;
using TTX.Core.Interfaces;
using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Core.Services;

public interface IUserService
{
    Task<User> ProcessOAuth(string oauthCode);
    Task<User> FindOrCreate(string twitchUsername);
    Task<User?> GetDetails(string username);
    Task<Pagination<User>> GetPaginated(
        int page = 1,
        int limit = 10,
        Order[]? order = null,
        Search? search = null
    );
}

public class UserService(IUserRepository repository, ITwitchService twitchService) : IUserService
{
    public Task<User?> GetDetails(string username) => repository.GetDetails(username);

    public Task<Pagination<User>> GetPaginated(int page = 1, int limit = 10, Order[]? order = null, Search? search = null) => repository.GetPaginated(page, limit, order, search);

    public async Task<User> FindOrCreate(string twitchUsername)
    {
        var user = await repository.GetDetails(twitchUsername);
        if (user is not null)
            return user;

        var newUser = await twitchService.Find(twitchUsername).ContinueWith(t =>
        {
            if (t.Result is null)
                throw new TwitchUserNotFoundException();

            return User.Create(t.Result);
        });

        repository.Add(newUser);
        await repository.SaveChanges();

        return newUser;
    }

    public async Task<User> ProcessOAuth(string oauthCode)
    {
        var tUser = await twitchService.GetByOAuth(oauthCode) ?? throw new TwitchUserNotFoundException();
        var user = await repository.GetDetails(tUser.Login);
        if (user is not null)
            return user;

        var newUser = User.Create(tUser);
        repository.Add(newUser);
        await repository.SaveChanges();

        return newUser;
    }
}