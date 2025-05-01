using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Dto.Players
{
    public class PlayerPartialDto : UserDto
    {
        [JsonPropertyName("credits")] public required long Credits { get; init; }

        [JsonPropertyName("type")] public required PlayerType Type { get; init; }

        public static PlayerPartialDto Create(Player player)
        {
            return new PlayerPartialDto
            {
                Id = player.Id,
                Name = player.Name,
                Slug = player.Slug,
                TwitchId = player.TwitchId,
                Credits = player.Credits,
                Type = player.Type,
                AvatarUrl = player.AvatarUrl.ToString(),
                CreatedAt = player.CreatedAt,
                UpdatedAt = player.UpdatedAt
            };
        }
    }
}