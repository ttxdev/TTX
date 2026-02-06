using TTX.Domain.Exceptions;

namespace TTX.App.Services.LootBoxes.Exceptions;

public sealed class LootBoxOpenedException() : InvalidActionException("Lootbox is already opened.");
