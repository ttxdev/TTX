using TTX.Core.Exceptions;
using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Core.Services;

public interface IOrderService
{
    Task<Transaction> PlaceOrder(string username, string creatorSlug, TransactionAction action, int amount);
}

public class OrderService(IUserRepository userRepository, ICreatorRepository creatorRepository) : IOrderService
{
    public async Task<Transaction> PlaceOrder(string username, string creatorSlug, TransactionAction action, int amount)
    {
        var user = await userRepository.GetDetails(username) ?? throw new UserNotFoundException();
        var creator = await creatorRepository.FindBySlug(creatorSlug) ?? throw new CreatorNotFoundException();

        var tx = action == TransactionAction.Buy
              ? user.Buy(creator, amount)
              : user.Sell(creator, amount);

        userRepository.Update(user);
        await userRepository.SaveChanges();

        return tx;
    }
}