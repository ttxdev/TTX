using TTX.Domain.Exceptions;
using TTX.Domain.ValueObjects;

namespace TTX.Domain.Models;

public class CreatorApplication : Model
{
    public required ModelId SubmitterId { get; init; }
    public required Platform Platform { get; init; }
    public required PlatformId PlatformId { get; init; }
    public required Ticker Ticker { get; init; }
    public required Name Name { get; init; }
    public CreatorApplicationStatus Status { get; private set; } = CreatorApplicationStatus.Pending;

    public virtual Player Submitter { get; init; } = null!;

    public static CreatorApplication Create(
        Name name,
        Ticker ticker,
        Platform platform,
        PlatformId platformId,
        Player submitter
    )
    {
        return new CreatorApplication
        {
            Name = name,
            Platform = platform,
            PlatformId = platformId,
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
