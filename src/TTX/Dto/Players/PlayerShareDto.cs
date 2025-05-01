using System.Text.Json.Serialization;
using TTX.Dto.Creators;
using TTX.Models;

namespace TTX.Dto.Players
{
    public class PlayerShareDto(Share share)
    {
        [JsonPropertyName("creator")] public CreatorPartialDto Creator { get; } = new(share.Creator);

        [JsonPropertyName("quantity")] public int Quantity { get; } = share.Quantity;
    }
}