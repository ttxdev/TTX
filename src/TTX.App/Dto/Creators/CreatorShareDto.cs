using System.Text.Json.Serialization;
using TTX.App.Dto.Players;
using TTX.Domain.Models;

namespace TTX.App.Dto.Creators;

public class CreatorShareDto
{
    [JsonPropertyName("player")] public required PlayerPartialDto Player { get; init; }
    [JsonPropertyName("quantity")] public int Quantity { get; init; }

    public static CreatorShareDto Create(Share share)
    {
        return new CreatorShareDto { Player = PlayerPartialDto.Create(share.Player), Quantity = share.Quantity };
    }
}
