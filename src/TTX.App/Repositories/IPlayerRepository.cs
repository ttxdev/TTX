using System.Runtime.CompilerServices;
using TTX.App.Dto.Pagination;
using TTX.App.Dto.Portfolio;
using TTX.App.Services.Players;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;

namespace TTX.App.Repositories;

public interface IPlayerRepository
{
    void Add(Player player);
    Task<Player?> FindByPlatform(Platform platform, PlatformId id);
    Task<Player?> GetPlayerData(Slug slug, HistoryParams historyParams);
    Task<IndexResult<Player>> Index(
        int page,
        int limit,
        string? search,
        Order<PlayerOrderBy> order,
        HistoryParams history
    );
    Task SaveChanges();
    ConfiguredCancelableAsyncEnumerable<Player> SeekAll(CancellationToken stoppingToken = default);
    void Update(Player player);
}
