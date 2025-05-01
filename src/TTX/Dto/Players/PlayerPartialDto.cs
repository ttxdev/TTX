using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Dto.Players
{
    public class PlayerPartialDto(Player user) : UserDto(user)
    {
        [JsonPropertyName("credits")] public long Credits { get; } = user.Credits;

        [JsonPropertyName("type")] public PlayerType Type { get; } = user.Type;
    }
}