using System.Text.Json.Serialization;
using TTX.Core.Models;

namespace TTX.Interface.Api.Dto;

public class CreatorShareDto(Share share)
{
    [JsonPropertyName("user")]
    public UserPartialDto User { get; } = new UserPartialDto(share.User);
    [JsonPropertyName("quantity")]
    public int Quantity { get; } = share.Quantity;
}