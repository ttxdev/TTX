using TTX.Core.Models;

namespace TTX.Core.Interfaces;

public interface ISessionService
{
    int? CurrentUserId { get; set; }
    bool IsAdmin();
    Task<User?> GetUser();
}
