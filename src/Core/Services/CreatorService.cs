using TTX.Core.Exceptions;
using TTX.Core.Interfaces;
using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Core.Services;

public interface ICreatorService
{
    Task<Creator> Onboard(string username, string ticker);
    Task<Creator[]> GetAllAbove(long value);
    Task<Creator?> GetDetails(string slug, TimeStep step = TimeStep.FiveMinute, DateTimeOffset? after = null);
    Task<Vote[]?> GetHistory(string slug, TimeStep step = TimeStep.Hour, DateTimeOffset? after = null);
    Task<Vote[]?> GetLatestVotes(string slug, DateTimeOffset after, TimeStep step = TimeStep.Hour);
    Task<Pagination<Creator>> GetPaginated(
        int page = 1,
        int limit = 10,
        Order[]? order = null,
        Search? search = null
    );
}

public class CreatorService(ISessionService sessionService, ITwitchService twitch, IVoteRepository voteRepo, ICreatorRepository r) : Service<Creator>(r), ICreatorService
{
    public async Task<Pagination<Creator>> GetPaginated(
      int page = 1,
      int limit = 10,
      Order[]? order = null,
      Search? search = null
    )
    {
        var creators = await r.GetPaginated(page, limit, order, search);
        var creatorHistories = await voteRepo.GetAllFor([.. creators.Data.Select(c => c.Id)], TimeStep.ThirtyMinute, DateTimeOffset.UtcNow.AddDays(-1));
        foreach (var entry in creatorHistories)
        {
            var creator = creators.Data.FirstOrDefault(c => c.Id == entry.Key);
            if (creator is null) continue;

            creator.History = [.. entry.Value];
        }

        return creators;
    }

    public async Task<Creator> Onboard(string username, string ticker)
    {
        var user = await sessionService.GetUser();
        if (user is null || !user.IsAdmin()) throw new UnauthorizedException();

        var creator = await twitch.Find(username).ContinueWith(t =>
        {
            if (t.Result is null)
                throw new TwitchUserNotFoundException();

            return Creator.Create(t.Result, ticker);
        });

        repository.Add(creator);
        await repository.SaveChanges();

        return creator;
    }

    public Task<Creator[]> GetAllAbove(long value) => r.GetAllAbove(value);
    public async Task<Creator?> GetDetails(
        string slug,
        TimeStep step = TimeStep.Minute,
        DateTimeOffset? after = null
    )
    {
        var creator = await r.GetDetails(slug);
        if (creator is null) return null;

        creator.History = [.. voteRepo.GetAll(creator.Id, step, after ?? DateTimeOffset.UtcNow.AddHours(-1)).Result];
        return creator;
    }


    public async Task<Vote[]?> GetHistory(string slug, TimeStep step = TimeStep.Minute, DateTimeOffset? after = null)
    {
        var creatorId = await r.GetId(slug);
        if (creatorId is null) return null;

        return await voteRepo.GetAll(creatorId.Value!, step, after ?? DateTimeOffset.UtcNow.AddHours(-1));
    }

    public async Task<Vote[]?> GetLatestVotes(string slug, DateTimeOffset after, TimeStep step = TimeStep.Minute)
    {
        var creatorId = await r.GetId(slug);
        if (creatorId is null) return null;

        return await voteRepo.GetLatestVotes(creatorId.Value, after, step);
    }
}