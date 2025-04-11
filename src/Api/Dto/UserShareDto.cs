using System.Text.Json.Serialization;
using TTX.Core.Models;

namespace TTX.Interface.Api.Dto;

public class UserShareDto(Share share)
{
    [JsonPropertyName("creator")]
    public CreatorPartialDto Creator { get; } = new CreatorPartialDto(share.Creator);
    [JsonPropertyName("quantity")]
    public int Quantity { get; } = share.Quantity;
}