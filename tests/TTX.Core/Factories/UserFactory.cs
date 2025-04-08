using Bogus;
using TTX.Core.Models;

namespace TTX.Tests.Core.Factories;

public class UserFactory
{
    public static User Create(
        int? id = 0,
        string? name = null,
        string? avatarUrl = null,
        int? credits = null,
        DateTime? createdAt = null
    )
    {
        var faker = new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Int(1, 1000))
            .RuleFor(u => u.Name, f => name ?? f.Internet.UserName())
            .RuleFor(u => u.AvatarUrl, f => avatarUrl ?? f.Internet.Avatar())
            .RuleFor(u => u.Credits, f => credits ?? f.Random.Int(0, 10000))
            .RuleFor(u => u.CreatedAt, f => f.Date.Past(1));

        return faker.Generate();
    }
}