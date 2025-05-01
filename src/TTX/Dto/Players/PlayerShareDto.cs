using System.Text.Json.Serialization;
using TTX.Dto.Creators;
using TTX.Models;

namespace TTX.Dto.Players
{
    public class PlayerShareDto
    {
        [JsonPropertyName("creator")] public required CreatorPartialDto Creator { get; init; }

        [JsonPropertyName("quantity")] public required int Quantity { get; init; }

        public static PlayerShareDto Create(Share share)
        {
            return new PlayerShareDto { Creator = CreatorPartialDto.Create(share.Creator), Quantity = share.Quantity };
        }
    }
}