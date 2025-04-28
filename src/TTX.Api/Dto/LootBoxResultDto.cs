using System.Text.Json.Serialization;
using TTX.Commands.LootBoxes.OpenLootBox;

namespace TTX.Api.Dto;

public class LootBoxResultDto(OpenLootBoxResult result)
{
    [JsonPropertyName("result")] public CreatorRarityDto Result { get; } = new(result.Result);

    [JsonPropertyName("rarities")]
    public CreatorRarityDto[] Rarities { get; } = [.. result.Rarities.Select(x => new CreatorRarityDto(x))];
}