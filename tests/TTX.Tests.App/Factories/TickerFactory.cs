using Bogus;
using TTX.Domain.ValueObjects;

namespace TTX.Tests.App.Factories;

public class TickerFactory(Faker _faker)
{
    public Ticker Create()
    {
        return string.Join("", _faker.Random.Chars('A', 'Z'));
    }
}
