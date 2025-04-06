using TTX.Core.Models;

namespace TTX.Core.Repositories;

public enum TimeStep
{
    Minute,
    FifteenMinute,
    ThirtyMinute,
    Hour,
    Day,
    Week,
    Month
}

public interface IVoteRepository
{
    Task RecordVote(Vote vote);
    Task<Vote[]> GetAll(int creatorId, TimeStep step = TimeStep.Hour, DateTimeOffset? after = null);
}