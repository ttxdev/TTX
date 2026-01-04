using TTX.Domain.Models;
using TTX.Domain.ValueObjects;

namespace TTX.App.Repositories;

public interface ITransactionRepository
{
    Task<Creator?> FindCreator(Slug creatorSlug);
    Task<Player?> FindPlayerWithLootbox(ModelId playerId, ModelId boxId);
    Task<Player?> FindPlayerWithTransactions(ModelId actorId);
    Task<Creator[]> GetCreatorsByMinValue(int lootBoxMinValue);
    Task SaveChanges();
}
