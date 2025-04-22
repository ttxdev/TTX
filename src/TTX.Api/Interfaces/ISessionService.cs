using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Api.Interfaces;

public interface ISessionService
{
    public Slug? GetCurrentUserSlug();
    public string GetTwitchLoginUrl();
    public string CreateSession(Player player);
}
