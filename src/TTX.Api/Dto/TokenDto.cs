using System.Text.Json.Serialization;

namespace TTX.Api.Dto;

public class TokenDto(string token)
{
    [JsonPropertyName("access_token")] public string Token { get; } = token;
}