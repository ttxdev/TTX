using TTX.Core.Exceptions;
using TTX.Core.Interfaces;
using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Core.Services;

public interface ICreatorService : IService<Creator>
{
    Task<Creator> Onboard(string username, string ticker);
    Task<Creator[]> GetAllAbove(long value);
    Task<Creator?> FindBySlug(string slug);
    Task<Vote[]> GetHistory(int creatorId, TimeStep step = TimeStep.Hour, DateTimeOffset? after = null);
}

public class CreatorService(ISessionService sessionService, ITwitchService twitch, IVoteRepository voteRepo, ICreatorRepository r) : Service<Creator>(r), ICreatorService
{
    public async Task<Creator> Onboard(string username, string ticker)
    {
        if (!sessionService.IsAdmin()) throw new UnauthorizedException();

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

    public async Task<Creator[]> GetAllAbove(long value)
    {
        if (!sessionService.IsAdmin()) throw new UnauthorizedException();

        var creators = await r.GetAllAbove(value);
        return creators;
    }

    public Task<Creator?> FindBySlug(string slug) => r.FindBySlug(slug);


    public Task<Vote[]> GetHistory(int creatorId, TimeStep step = TimeStep.Hour, DateTimeOffset? after = null)
    {
        return voteRepo.GetAll(creatorId, step, after);
    }
}