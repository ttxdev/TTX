using Bogus;
using TTX.ValueObjects;

namespace TTX.Tests.Factories;

public class TwitchIdFactory
{
    public static TwitchId Create()
    {
        Faker faker = new();

        return TwitchId.Create(faker.Random.Int(1, 1000000).ToString());
    }
}
