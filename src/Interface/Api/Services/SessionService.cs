using System.Security.Claims;
using TTX.Core.Interfaces;
using TTX.Core.Models;
using TTX.Core.Repositories;
using TTX.Interface.Api.Provider;

namespace TTX.Interface.Api.Services;

public class SessionService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor) : ISessionService
{
    public int? CurrentUserId { get; set; }

    private string? Slug => httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.Name);

    public async Task<User?> GetUser()
    {
        if (Slug is null)
            return null;

        return await userRepository.GetDetails(Slug);
    }
}