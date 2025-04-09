using TTX.Core.Exceptions;
using TTX.Core.Interfaces;
using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Core.Services;

public interface ICreatorService
{
    Task<Creator?> GetDetails(string slug, TimeStep step = TimeStep.FiveMinute, DateTimeOffset? after = null);
    Task<Pagination<Creator>> GetPaginated(
        int page = 1,
        int limit = 10,
        Order[]? order = null,
        Search? search = null
    );
    Task RecordValue(string slug, int value);
    Task<Vote[]?> PullLatestHistory(string slug, DateTimeOffset after, TimeStep step = TimeStep.Hour);
    Task<Creator?> UpdateStreamInfo(int id, StreamStatus status);
    Task<Creator> Onboard(TwitchUser tUser, string ticker);
}

public class CreatorService(ICreatorRepository repository) : ICreatorService
{
    public async Task<Pagination<Creator>> GetPaginated(
      int page = 1,
      int limit = 10,
      Order[]? order = null,
      Search? search = null
    )
    {
        var creators = await repository.GetPaginated(
            page,
            limit,
            new HistoryParams
            {
                Step = TimeStep.FiveMinute,
                After = DateTimeOffset.UtcNow.AddHours(-1)
            },
            order,
            search);

        return creators;
    }

    public async Task<Creator?> GetDetails(
        string slug,
        TimeStep step = TimeStep.Minute,
        DateTimeOffset? after = null
    )
    {
        var creator = await repository.GetDetails(slug, new HistoryParams
        {
            Step = step,
            After = after ?? DateTimeOffset.UtcNow.AddHours(-1)
        });
        if (creator is null) return null;

        return creator;
    }

    public Task<Vote[]?> PullLatestHistory(string slug, DateTimeOffset after, TimeStep step = TimeStep.Minute)
    {
        return repository.PullLatestHistory(slug, new HistoryParams
        {
            Step = step,
            After = after
        });
    }

    public async Task RecordValue(string slug, int netChange)
    {
        var creator = await repository.FindBySlug(slug) ?? throw new CreatorNotFoundException();
        var vote = creator.CreateVote(netChange);
        await repository.RecordValue(creator, vote);
    }

    public Task<Creator?> UpdateStreamInfo(int id, StreamStatus status) =>
      repository.UpdateStreamInfo(id, status);

    public async Task<Creator> Onboard(TwitchUser tUser, string ticker)
    {
        var creator = Creator.Create(tUser, ticker);
        repository.Add(creator);
        await repository.SaveChanges();

        return creator;
    }
}