using TTX.Domain.ValueObjects;
using TTX.Domain.Exceptions;

namespace TTX.Domain.Models;

public class LootBox : Model
{
    public required ModelId PlayerId { get; init; }
    public ModelId? ResultId { get; private set; }
    public bool IsOpen => ResultId is not null;
    public virtual Player Player { get; init; } = null!;
    public Creator? Result { get; private set; }

    public static LootBox Create(Player player)
    {
        return new LootBox { Player = player, PlayerId = player.Id };
    }

    public void SetResult(Creator creator)
    {
        if (IsOpen)
        {
            throw new InvalidActionException("Lootbox is already opened.");
        }

        Result = creator;
        ResultId = creator.Id;
    }
}
