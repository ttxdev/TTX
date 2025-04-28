using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Api.Dto;

public class PlayerShareDto(Share share)
{
    [JsonPropertyName("creator")] public CreatorPartialDto Creator { get; } = new(share.Creator);

    [JsonPropertyName("quantity")] public int Quantity { get; } = share.Quantity;
}