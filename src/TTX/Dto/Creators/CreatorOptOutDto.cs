using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Dto.Creators
{
    public class CreatorOptOutDto : BaseDto
    {
        [JsonPropertyName("twitch_id")] public required string TwitchId { get; init; }

        public static CreatorOptOutDto Create(CreatorOptOut opt)
        {
            return new CreatorOptOutDto
            {
                Id = opt.Id,
                TwitchId = opt.TwitchId,
                CreatedAt = opt.CreatedAt,
                UpdatedAt = opt.UpdatedAt
            };
        }
    }
}
