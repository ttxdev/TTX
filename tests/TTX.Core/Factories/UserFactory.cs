using Bogus;
using TTX.Core.Models;

namespace TTX.Tests.Core.Factories;

public class UserFactory
{
    public static User Create()
    {
        var faker = new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Int(1, 1000))
            .RuleFor(u => u.Name, f => f.Person.FullName)
            .RuleFor(u => u.AvatarUrl, f => f.Internet.Avatar())
            .RuleFor(u => u.Credits, f => f.Random.Int(0, 10000))
            .RuleFor(u => u.CreatedAt, f => f.Date.Past(1));

        return faker.Generate();
    }
}