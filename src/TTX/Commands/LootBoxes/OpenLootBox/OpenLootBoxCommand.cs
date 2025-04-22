using TTX.ValueObjects;

namespace TTX.Commands.LootBoxes.OpenLootBox;

public readonly struct OpenLootBoxCommand : ICommand<OpenLootBoxResult>
{
    public required Slug ActorSlug { get; init; }
}
