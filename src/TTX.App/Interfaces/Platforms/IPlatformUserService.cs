using TTX.Domain.ValueObjects;

namespace TTX.App.Interfaces.Platforms;

public interface IPlatformUserService
{
    Task<PlatformUser?> GetUserByUsername(Slug username);
    Task<PlatformUser?> GetUserById(PlatformId id);
    Task<PlatformUser?> ResolveByOAuth(string code);
    Uri GetLoginUrl();
}
