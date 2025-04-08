using Bogus;
using TTX.Core.Models;

namespace TTX.Tests.Core.Factories;

public class TransactionFactory
{
    public static Transaction CreateBuy(User user, Creator creator, int? amount = null) => Create(user, creator, amount, TransactionAction.Buy);
    public static Transaction CreateSell(User user, Creator creator, int? amount = null) => Create(user, creator, amount, TransactionAction.Sell);
    public static Transaction Create(User user, Creator creator, int? amount = null, TransactionAction? action = null)
    {
        var faker = new Faker<Transaction>()
            .RuleFor(t => t.UserId, user.Id)
            .RuleFor(t => t.CreatorId, creator.Id)
            .RuleFor(t => t.User, user)
            .RuleFor(t => t.Creator, creator)
            .RuleFor(t => t.Quantity, f => amount ?? f.Random.Int(1, 100))
            .RuleFor(t => t.Action, f => action ?? f.PickRandom<TransactionAction>())
            .RuleFor(t => t.Value, f => creator.Value);

        return faker.Generate();
    }
}