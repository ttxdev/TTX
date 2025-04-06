using Microsoft.EntityFrameworkCore;
using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Infrastructure.Data.Repositories;

public class CreatorRepository(ApplicationDbContext context) : RepositoryBase<Creator>(context), ICreatorRepository
{
    public Task<Creator?> FindBySlug(string slug) => FindQuery().FirstOrDefaultAsync(c => c.Slug == slug);
    public Task<Creator[]> GetAllAbove(long value) => DbSet.Where(c => c.Value > value).ToArrayAsync();
    public Task<long> GetValueSum() => DbSet.SumAsync(c => c.Value);

    public override async ValueTask<Creator?> Find(int id)
    {
        return await FindQuery().SingleOrDefaultAsync(c => c.Id == id);
    }

    private IQueryable<Creator> FindQuery()
    {
        return DbSet
            .Include(c => c.Transactions)
            .ThenInclude(t => t.User)
            .Include(c => c.Transactions);
    }
}