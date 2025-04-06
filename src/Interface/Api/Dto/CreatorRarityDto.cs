using System.Text.Json.Serialization;
using TTX.Core.Models;

namespace TTX.Interface.Api.Dto;

public class CreatorRarityDto(CreatorRarity creatorRarity)
{
  [JsonPropertyName("creator")]
  public CreatorPartialDto Creator { get; } = new(creatorRarity.Creator);
  [JsonPropertyName("rarity")]
  public Rarity Rarity { get; } = creatorRarity.Rarity;
}