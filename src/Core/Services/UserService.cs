using TTX.Core.Exceptions;
using TTX.Core.Interfaces;
using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Core.Services;

public interface IUserService : IService<User>
{
    Task<User> FindOrCreate(string twitchUsername);
    Task<User> OAuthOnboard(string oauthCode);
    Task<User?> FindByName(string username);
    Task<Transaction> PlaceOrder(Creator creator, TransactionAction action, int amount);
    Task<LootBoxResult> Gamba();
}

public class UserService(ISessionService sessionService, ITwitchService twitchService, ICreatorRepository creatorRepository, IUserRepository repo) : Service<User>(repo), IUserService
{
    public async Task<User> OAuthOnboard(string oauthCode)
    {
        var tUser = await twitchService.GetByOAuth(oauthCode) ?? throw new TwitchUserNotFoundException();
        var user = User.Create(tUser);
        repository.Add(user);
        await repository.SaveChanges();

        return user;
    }

    public async Task<Transaction> PlaceOrder(Creator creator, TransactionAction action, int amount)
    {
        var user = RequireUser();
        var tx = action == TransactionAction.Buy
          ? user.Buy(creator, amount)
          : user.Sell(creator, amount);

        repository.Update(user);
        await repository.SaveChanges();

        return tx;
    }

    public async Task<LootBoxResult> Gamba()
    {
        var user = RequireUser();
        var creators = await creatorRepository.GetAllAbove(100);
        var lootBox = user.Gamba(creators);

        repository.Update(user);
        await repository.SaveChanges();

        return lootBox;
    }

    public Task<User?> FindByName(string username) => repo.FindByName(username);

    private User RequireUser() => sessionService.CurrentUser ?? throw new AuthenticationRequiredException();

    public async Task<User> FindOrCreate(string twitchUsername)
    {
        var user = await repo.FindByName(twitchUsername);
        if (user is not null)
            return user;

        return await twitchService.Find(twitchUsername).ContinueWith(t =>
        {
            if (t.Result is null)
                throw new TwitchUserNotFoundException();

            return Onboard(t.Result).Result;
        });
    }

    private async Task<User> Onboard(TwitchUser tUser)
    {
        var user = User.Create(tUser);
        repository.Add(user);
        await repository.SaveChanges();

        return user;
    }
}