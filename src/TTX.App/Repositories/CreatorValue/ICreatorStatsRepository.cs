using TTX.Domain.ValueObjects;

namespace TTX.App.Repositories.CreatorValue;

public interface ICreatorStatsRepository
{
    Task<CreatorStats> GetByCreator(Slug slug);
    Task SetByCreator(Slug slug, CreatorStats stats);
    Task<CreatorStats[]> GetAll(bool clear = true);
}
