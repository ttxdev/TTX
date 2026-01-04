using System.Runtime.CompilerServices;
using TTX.App.Dto.Pagination;
using TTX.App.Dto.Portfolio;
using TTX.App.Services.Creators;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;

namespace TTX.App.Repositories;

public interface ICreatorRepository
{
    Task<bool> IsTickerTaken(Ticker ticker);
    Task<IndexResult<Creator>> Index(
        int page,
        int limit,
        int? minValue,
        string? search,
        Order<CreatorOrderBy> order,
        HistoryParams history
    );
    Task<Creator?> GetCreatorDetails(Slug slug, HistoryParams historyParams);
    Task<bool?> IsPlayer(Slug creatorSlug, ModelId playerId);
    Task<Creator?> FindBySlug(Slug slug);
    void AddOptOut(CreatorOptOut opt);
    void Remove(Creator creator);
    Task SaveChanges();
    Task<Creator?> FindByPlatform(Platform platform, PlatformId platformId);
    void Add(Creator creator);
    Task<bool> IsOptOut(Platform platform, PlatformId platformId);
    IAsyncEnumerable<Creator> GetAll();
    Task<Creator?> Find(ModelId creatorId);
    Task StoreVote(Vote vote);
    void Update(Creator creator);
}
