using Microsoft.EntityFrameworkCore;
using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Infrastructure.Data.Repositories;

public class UserRepository(ApplicationDbContext context) : RepositoryBase<User>(context), IUserRepository
{
    public Task<User?> FindByName(string name)
    {
        return FindQuery().FirstOrDefaultAsync(x => x.Name == name);
    }

    public Task<User?> FindByTwitchId(string twitchId) => DbSet.FirstOrDefaultAsync(x => x.TwitchId == twitchId);

    public override async ValueTask<User?> Find(int id)
    {
        return await FindQuery().FirstOrDefaultAsync(u => u.Id == id);
    }

    private IQueryable<User> FindQuery()
    {
        return DbSet
            .Include(u => u.Transactions.OrderByDescending(t => t.CreatedAt))
            .ThenInclude(t => t.Creator)
            .Include(u => u.LootBoxes)
            .ThenInclude(l => l.Result);
    }
}