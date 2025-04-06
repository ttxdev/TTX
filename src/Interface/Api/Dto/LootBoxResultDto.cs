using System.Text.Json.Serialization;
using TTX.Core.Models;

namespace TTX.Interface.Api.Dto;

public class LootBoxResultDto(LootBoxResult result)
{
    [JsonPropertyName("result")]
    public CreatorRarityDto Result { get; } = new(result.Result);
    [JsonPropertyName("rarities")]
    public CreatorRarityDto[] Rarities { get; } = [.. result.Rarities.Select(x => new CreatorRarityDto(x))];
}