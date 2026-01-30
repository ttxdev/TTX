using TTX.Domain.ValueObjects;
using TTX.Domain.Platforms;

namespace TTX.App.Interfaces.Platforms;

public interface IPlatformUserService
{
    Task<PlatformUser?> GetUserByUsername(Slug username);
    Task<PlatformUser?> GetUserById(PlatformId id);
    Task<PlatformUser?> ResolveByOAuth(string code);
    Uri GetLoginUrl();
}
