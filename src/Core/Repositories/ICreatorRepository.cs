using TTX.Core.Models;

namespace TTX.Core.Repositories;

public enum TimeStep
{
    Minute,
    FiveMinute,
    FifteenMinute,
    ThirtyMinute,
    Hour,
    Day,
    Week,
    Month
}

public struct HistoryParams
{
    public required TimeStep Step { get; set; }
    public required DateTimeOffset After { get; set; }
}

public interface ICreatorRepository : IRepository<Creator>
{
    Task<Creator?> FindBySlug(string slug);
    Task<long> GetValueSum();
    Task<Creator[]> GetAllAbove(long value);
    Task<Creator[]> GetAll();
    Task<Creator[]> GetAllByIds(int[] ids);
    Task<Creator?> GetDetails(string slug, HistoryParams history);
    Task<Vote[]?> PullLatestHistory(string slug, HistoryParams history);
    Task<int?> GetId(string slug);
    Task<Creator?> UpdateStreamInfo(int id, StreamStatus status);
    Task RecordValue(Creator creator, Vote vote);
    Task<Pagination<Creator>> GetPaginated(
        int page,
        int limit,
        HistoryParams history,
        Order[]? order = null,
        Search? search = null
    );
}