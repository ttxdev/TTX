using System.Text.Json.Serialization;
using TTX.Core.Models;

namespace TTX.Interface.Api.Dto;

public class LootBoxDto(LootBox lootBox) : BaseDto<LootBox>(lootBox)
{
    [JsonPropertyName("is_open")]
    public bool IsOpen { get; } = lootBox.IsOpen;
    [JsonPropertyName("result")]
    public CreatorPartialDto? ResultId { get; } = lootBox.ResultId is not null
        ? new CreatorPartialDto(lootBox.Result!)
        : null;
    [JsonPropertyName("user_id")]
    public int UserId { get; } = lootBox.UserId;
}