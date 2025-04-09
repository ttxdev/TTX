using TTX.Core.Exceptions;
using TTX.Core.Interfaces;
using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Core.Services;

public interface IUserService
{
    Task<User> FindOrCreate(string twitchUsername);
    Task<User> ProcessOAuth(string oauthCode);
    Task<DiscordUser> ProcessDiscordOAuth(string oauthCode);
    Task<User> ProcessDiscordToTwitchOAuth(string oauthCode, string twitchId);
    Task<User?> GetDetails(string username);
    Task<Transaction> PlaceOrder(Creator creator, TransactionAction action, int amount);
    Task<LootBoxResult> Gamba();
    Task<Pagination<User>> GetPaginated(
        int page = 1,
        int limit = 10,
        Order[]? order = null,
        Search? search = null
    );
}

public class UserService(ISessionService sessionService, ITwitchService twitchService, IDiscordService discordService, ICreatorRepository creatorRepository, IUserRepository repo) : Service<User>(repo), IUserService
{
    public async Task<User> ProcessOAuth(string oauthCode)
    {
        var tUser = await twitchService.GetByOAuth(oauthCode) ?? throw new TwitchUserNotFoundException();
        var user = await repo.GetDetails(tUser.DisplayName);
        if (user is not null)
            return user;

        var newUser = User.Create(tUser);
        repository.Add(newUser);
        await repository.SaveChanges();

        return newUser;
    }

    public async Task<DiscordUser> ProcessDiscordOAuth(string oauthCode)
    {
        return await discordService.GetByOAuth(oauthCode, twitchService) ?? throw new DiscordUserNotFoundException();
    }

    public async Task<User> ProcessDiscordToTwitchOAuth(string oauthCode, string twitchId)
    {
        var tUser = await discordService.GetByOAuthToTwitch(oauthCode, twitchId, twitchService) ?? throw new TwitchUserNotFoundException();
        var user = await repo.GetDetails(tUser.DisplayName);
        if (user is not null)
            return user;

        var newUser = User.Create(tUser);
        repository.Add(newUser);
        await repository.SaveChanges();

        return newUser;
    }

    public async Task<Transaction> PlaceOrder(Creator creator, TransactionAction action, int amount)
    {
        var user = await RequireUser();
        var tx = action == TransactionAction.Buy
          ? user.Buy(creator, amount)
          : user.Sell(creator, amount);

        repository.Update(user);
        await repository.SaveChanges();

        return tx;
    }

    public async Task<LootBoxResult> Gamba()
    {
        var user = await RequireUser();
        var creators = await creatorRepository.GetAllAbove(100);
        var lootBox = user.Gamba(creators);

        repository.Update(user);
        await repository.SaveChanges();

        return lootBox;
    }

    public Task<User?> GetDetails(string username) => repo.GetDetails(username);

    private async Task<User> RequireUser() => await sessionService.GetUser() ?? throw new AuthenticationRequiredException();

    public async Task<User> FindOrCreate(string twitchUsername)
    {
        var user = await repo.GetDetails(twitchUsername);
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

    public Task<Pagination<User>> GetPaginated(int page = 1, int limit = 10, Order[]? order = null, Search? search = null) => repo.GetPaginated(page, limit, order, search);
}