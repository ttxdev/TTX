using TTX.Core.Models;

namespace TTX.Core.Interfaces;

public interface ISessionService
{
    User? CurrentUser { get; set; }
    bool IsAdmin();
}
