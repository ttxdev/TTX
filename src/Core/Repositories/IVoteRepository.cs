using TTX.Core.Models;

namespace TTX.Core.Repositories;

public enum TimeStep
{
    Minute,
    FiveMinute,
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
    Task<Dictionary<int, Vote[]>> GetAllFor(int[] creatorIds, TimeStep step = TimeStep.Hour, DateTimeOffset? after = null);
    Task<Vote[]> GetLatestVotes(int creatorId, DateTimeOffset after, TimeStep step = TimeStep.Hour);
}