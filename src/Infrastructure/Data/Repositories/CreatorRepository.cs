using Microsoft.EntityFrameworkCore;
using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Infrastructure.Data.Repositories;

public class CreatorRepository(ApplicationDbContext context) : RepositoryBase<Creator>(context), ICreatorRepository
{
    public Task<Creator[]> GetAll() => DbSet.ToArrayAsync();
    public Task<Creator[]> GetAllByIds(int[] ids) => DbSet.Where(c => ids.Contains(c.Id)).ToArrayAsync();
    public Task<Creator?> GetDetails(string slug) => DbSet
            .Include(c => c.Transactions.OrderByDescending(t => t.CreatedAt))
            .ThenInclude(t => t.User)
            .FirstOrDefaultAsync(c => c.Slug == slug);
    public Task<Creator[]> GetAllAbove(long value) => DbSet.Where(c => c.Value > value).ToArrayAsync();
    public Task<long> GetValueSum() => DbSet.SumAsync(c => c.Value);
    public async Task<Creator?> UpdateStreamInfo(int id, StreamStatus status)
    {
        var creator = DbSet.FirstOrDefault(c => c.Id == id);
        if (creator is null) return null;

        creator.StreamStatus = status;
        Update(creator);
        await SaveChanges();

        return creator;
    }

  public Task<Creator?> Find(int creatorId) => DbSet.FirstOrDefaultAsync(c => c.Id == creatorId);
}