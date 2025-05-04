using System.Collections.Immutable;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Models
{
    public class CreatorApplication : Model
    {

        public required TwitchId TwitchId { get; init; }
        public required Ticker Ticker { get; init; }
        public required CreatorApplicationStatus Status { get; init; }

        public static CreatorApplication Create(
            Ticker ticker,
            TwitchId twitchId
        )
        {
            return new CreatorApplication
            {
                TwitchId = twitchId,
                Ticker = ticker,
                Status = CreatorApplicationStatus.Pending,
            };
        }
    }

    public enum CreatorApplicationStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
