using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Api.Dto;

public class CreatorShareDto(Share share)
{
    [JsonPropertyName("player")]
    public PlayerPartialDto Player { get; } = new PlayerPartialDto(share.Player);
    [JsonPropertyName("quantity")]
    public int Quantity { get; } = share.Quantity;
}