using Bogus;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;

namespace TTX.Tests.App.Factories;

public class CreatorFactory(Faker _faker, NameFactory _nameFactory, SlugFactory _slugFactory, TickerFactory _tickerFactory)
{
    public Creator Create(
        Name? name = null,
        Slug? slug = null,
        Ticker? ticker = null,
        PlatformId? platformId = null,
        Credits? value = null)
    {
        return new()
        {
            Name = name ?? _nameFactory.Create(),
            Slug = slug ?? _slugFactory.Create(),
            Value = value ?? 0,
            PlatformId = platformId ?? _faker.Random.UInt().ToString(),
            Ticker = ticker ?? _tickerFactory.Create(),
            AvatarUrl = new Uri(_faker.Internet.Avatar())
        };
    }
}
