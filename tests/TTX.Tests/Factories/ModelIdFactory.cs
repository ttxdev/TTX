using Bogus;
using TTX.ValueObjects;

namespace TTX.Tests.Factories;

public class ModelIdFactory
{
    public static ModelId Create()
    {
        Faker faker = new();

        return ModelId.Create(faker.Random.Int(1, 1000000));
    }
}
