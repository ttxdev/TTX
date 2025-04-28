using TTX.ValueObjects;

namespace TTX.Models
{
    public class LootBox : Model
    {
        public required ModelId PlayerId { get; init; }
        public ModelId? ResultId { get; init; } = null;
        public Player Player { get; init; } = null!;
        public Creator? Result { get; init; } = null;
        public bool IsOpen => ResultId is not null;

        public static LootBox Create(Player player)
        {
            return new LootBox { Player = player, PlayerId = player.Id };
        }
    }
}