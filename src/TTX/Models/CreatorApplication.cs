using TTX.Exceptions;
using TTX.ValueObjects;

namespace TTX.Models
{
    public class CreatorApplication : Model
    {
        public required ModelId SubmitterId { get; init; }
        public required TwitchId TwitchId { get; init; }
        public required Ticker Ticker { get; init; }
        public required Name Name { get; init; }
        public CreatorApplicationStatus Status { get; private set; } = CreatorApplicationStatus.Pending;
        public Player Submitter { get; init; } = null!;

        public static CreatorApplication Create(
            Name name,
            Ticker ticker,
            TwitchId twitchId,
            Player submitter
        )
        {
            return new CreatorApplication
            {
                Name = name,
                TwitchId = twitchId,
                Ticker = ticker,
                Status = CreatorApplicationStatus.Pending,
                Submitter = submitter,
                SubmitterId = submitter.Id
            };
        }

        public void UpdateStatus(CreatorApplicationStatus status)
        {
            if (Status is CreatorApplicationStatus.Approved or CreatorApplicationStatus.Rejected)
            {
                throw new InvalidActionException("Creator Application was already reviewed.");
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