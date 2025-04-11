using System.Text.Json.Serialization;

namespace TTX.Interface.Api.Dto;

public class TokenDto
{
    [JsonPropertyName("access_token")]
    public required string Token { get; set; }
}