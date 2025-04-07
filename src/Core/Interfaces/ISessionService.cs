using TTX.Core.Models;

namespace TTX.Core.Interfaces;

public interface ISessionService
{
    int? CurrentUserId { get; set; }
    Task<User?> GetUser();
}
