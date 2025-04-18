using Microsoft.EntityFrameworkCore;
using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Infrastructure.Data.Repositories;

public class UserRepository(ApplicationDbContext context) : RepositoryBase<User>(context), IUserRepository
{
    public Task<User?> GetDetails(string name)
    {
        return DbSet
            .Include(u => u.Transactions.OrderByDescending(t => t.CreatedAt))
            .ThenInclude(t => t.Creator)
            .Include(u => u.LootBoxes)
            .ThenInclude(l => l.Result)
            .FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());
    }
}