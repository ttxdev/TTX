using System.Text.Json.Serialization;

namespace TTX.Api.Controllers.Dto;

public class TokenDto(string token)
{
    [JsonPropertyName("access_token")] public string Token { get; } = token;
}
