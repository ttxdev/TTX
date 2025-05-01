using System.Text.Json.Serialization;
using TTX.Dto.Players;
using TTX.Models;

namespace TTX.Dto.Creators
{
    public class CreatorShareDto(Share share)
    {
        [JsonPropertyName("player")] public PlayerPartialDto Player { get; } = new(share.Player);

        [JsonPropertyName("quantity")] public int Quantity { get; } = share.Quantity;
    }
}