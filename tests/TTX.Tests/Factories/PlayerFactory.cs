using Bogus;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Tests.Factories;

public class PlayerFactory
{
    public static Player Create(Credits? credits = null, bool includeId = false)
    {
        Faker faker = new();
        var name = NameFactory.Create();

        Faker<Player> player = new();

        if (includeId)
            player.RuleFor(p => p.Id, ModelIdFactory.Create());

        return player
            .RuleFor(p => p.Name, name)
            .RuleFor(p => p.Slug, name)
            .RuleFor(p => p.AvatarUrl, new Uri(faker.Internet.Avatar()))
            .RuleFor(p => p.TwitchId, TwitchIdFactory.Create())
            .RuleFor(p => p.Credits, credits ?? faker.Random.Int(Player.StarterCredits, 1000));
    }
}