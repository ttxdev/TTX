using TTX.App.Interfaces.Platforms;
using TTX.Domain.Platforms;
using TTX.Domain.ValueObjects;

namespace TTX.Tests.App.Infrastructure.Platforms;

public class PlatformUserService : IPlatformUserService
{
    private readonly Dictionary<PlatformId, PlatformUser> _users = [];

    public Task<PlatformUser?> GetUserById(PlatformId id)
    {
        return Task.FromResult(_users.TryGetValue(id, out var user) ? user : null);
    }

    public Task<PlatformUser?> GetUserByUsername(Slug slug)
    {
        return Task.FromResult(_users.Values.FirstOrDefault(u => u.Username == slug));
    }

    public Task<PlatformUser?> ResolveByOAuth(string code)
    {
        throw new NotImplementedException();
    }

    public Uri GetLoginUrl()
    {
        throw new NotImplementedException();
    }

    public void Inject(PlatformUser user)
    {
        lock (_users)
        {
            _users.Add(user.Id, user);
        }
    }
}
