using Bogus;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;

namespace TTX.Tests.App.Factories;

public class PlayerFactory(Faker _faker, NameFactory _nameFactory, SlugFactory _slugFactory)
{
    public Player Create(string? name = null, string? slug = null, Credits? credits = null)
    {
        return new()
        {
            Name = name ?? _nameFactory.Create(),
            Slug = slug ?? _slugFactory.Create(),
            Credits = credits ?? 0,
            PlatformId = _faker.Random.UInt().ToString(),
            AvatarUrl = new Uri(_faker.Internet.Avatar())
        };
    }
}
