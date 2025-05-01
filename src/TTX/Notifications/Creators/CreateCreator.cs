using TTX.Dto.Creators;
using TTX.Models;

namespace TTX.Notifications.Creators
{
    public class CreateCreator : CreatorPartialDto, INotification
    {
        public static new CreateCreator Create(Creator creator)
        {
            return new CreateCreator
            {
                Id = creator.Id,
                Name = creator.Name,
                Slug = creator.Slug,
                Ticker = creator.Ticker,
                TwitchId = creator.TwitchId,
                Value = creator.Value,
                StreamStatus = StreamStatusDto.Create(creator.StreamStatus),
                History = creator.History.Select(VoteDto.Create).ToArray(),
                AvatarUrl = creator.AvatarUrl.ToString(),
                UpdatedAt = creator.UpdatedAt,
                CreatedAt = creator.CreatedAt
            };
        }
    }
}