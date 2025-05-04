using System.Collections.Immutable;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Models
{
    public class CreatorApplication : Model
    {

        public required Slug Slug { get; init; }
        public required Ticker Ticker { get; init; }
        public required ApplicationStatus Status { get; init; }

        public static CreatorApplication Create(
            Slug slug,
            Ticker ticker
        )
        {
            return new CreatorApplication
            {
                Slug = slug,
                Ticker = ticker,
                Status = ApplicationStatus.Pending,
            };
        }
    }

    public enum ApplicationStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
