using TTX.ValueObjects;

namespace TTX.Models
{
    public class CreatorOptOut : Model
    {
        public required TwitchId TwitchId { get; init; }
        public static CreatorOptOut Create(Creator creator) 
        {
            return new CreatorOptOut { TwitchId = creator.TwitchId };
        }
    }

}