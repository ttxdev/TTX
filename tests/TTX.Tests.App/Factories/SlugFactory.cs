using Bogus;
using TTX.Domain.ValueObjects;

namespace TTX.Tests.App.Factories;

public class SlugFactory(Faker faker)
{
    public Slug Create()
    {
        return faker.Internet.UserName().Replace('.', '_').ToLower();
    }
}
