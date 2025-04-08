using Bogus;
using TTX.Core.Models;

namespace TTX.Tests.Core.Factories;

public class CreatorFactory
{
    public static Creator Create(string? name = null, string? slug = null, string? ticker = null)
    {
        var faker = new Faker<Creator>()
            .RuleFor(c => c.Name, f => name ?? f.Person.FullName)
            .RuleFor(c => c.Slug, f => slug ?? f.Internet.UserName())
            .RuleFor(c => c.Ticker, f => ticker ?? f.Commerce.ProductAdjective().ToUpper())
            .RuleFor(c => c.AvatarUrl, f => f.Internet.Avatar())
            .RuleFor(c => c.Value, f => f.Random.Int(1, 1000));

        return faker.Generate();
    }
}