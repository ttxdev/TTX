using System.Collections.Immutable;
using TTX.Exceptions;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Models
{
    public class CreatorApplication : Model
    {

        public required ModelId SubmitterId { get; init; }
        public required TwitchId TwitchId { get; init; }
        public required Ticker Ticker { get; init; }
        public CreatorApplicationStatus Status { get; private set; } = CreatorApplicationStatus.Pending;

        public static CreatorApplication Create(
            Ticker ticker,
            TwitchId twitchId,
            ModelId submitterId
        )
        {
            return new CreatorApplication
            {
                SubmitterId = submitterId,
                TwitchId = twitchId,
                Ticker = ticker,
                Status = CreatorApplicationStatus.Pending,
            };
        }

        public void UpdateStatus(CreatorApplicationStatus status)
        {
            if (Status is CreatorApplicationStatus.Approved or CreatorApplicationStatus.Rejected)
            {
                throw new CreatorApplicationAlreadyCompletedException();
            }

            Status = status;
        }
    }

    public enum CreatorApplicationStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
