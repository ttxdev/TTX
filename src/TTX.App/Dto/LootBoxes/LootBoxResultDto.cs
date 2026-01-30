using System.Text.Json.Serialization;
using TTX.App.Dto.Players;
using TTX.Domain.ValueObjects;

namespace TTX.App.Dto.LootBoxes;

public class LootBoxResultDto
{
    [JsonPropertyName("lootbox_id")] public required ModelId LootBoxId { get; init; }
    [JsonPropertyName("player")] public required PlayerPartialDto Player { get; init; }
    [JsonPropertyName("result")] public required CreatorRarityDto Result { get; init; }
    [JsonPropertyName("transaction_id")] public required ModelId TransactionId { get; init; }
    [JsonPropertyName("rarities")] public required CreatorRarityDto[] Rarities { get; init; }

    public static LootBoxResultDto Create(OpenLootBoxResult result, ModelId transactionId)
    {
        return new LootBoxResultDto
        {
            LootBoxId = result.LootBox.Id,
            Player = PlayerPartialDto.Create(result.LootBox.Player),
            Result = CreatorRarityDto.Create(result.Result),
            TransactionId = transactionId,
            Rarities = [.. result.Rarities.Select(CreatorRarityDto.Create)]
        };
    }
}
