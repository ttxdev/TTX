using TTX.Domain.Exceptions;

namespace TTX.App.Services.Transactions.Exceptions;

public sealed class LootBoxOpenedException() : InvalidActionException("Lootbox is already opened.");
