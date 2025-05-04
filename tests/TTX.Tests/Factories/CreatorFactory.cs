using Bogus;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Tests.Factories;

public class CreatorFactory
{
    public static Creator Create(Credits? value = null, Name? username = null, Ticker? ticker = null, bool includeId = false)
    {
        Faker faker = new();
        Faker<Creator> creator = new();

        if (includeId)
            creator.RuleFor(c => c.Id, ModelIdFactory.Create());

        var name = username ?? NameFactory.Create();

        return creator
            .RuleFor(c => c.Name, name)
            .RuleFor(c => c.Slug, (Slug)name)
            .RuleFor(c => c.Ticker, ticker ?? Ticker.Create(faker.Random.String2(Ticker.MaxLength).ToUpper()))
            .RuleFor(c => c.AvatarUrl, new Uri(faker.Internet.Avatar()))
            .RuleFor(c => c.TwitchId, TwitchIdFactory.Create())
            .RuleFor(c => c.Value, value ?? faker.Random.Int(Creator.StarterValue, 1000));
    }
}
