using TTX.ValueObjects;

namespace TTX.Commands.LootBoxes.OpenLootBox
{
    public readonly struct OpenLootBoxCommand : ICommand<OpenLootBoxResult>
    {
        public required ModelId ActorId { get; init; }
        public required ModelId LootBoxId { get; init; }
    }
}