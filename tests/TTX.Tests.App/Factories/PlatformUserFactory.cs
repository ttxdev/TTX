using Bogus;
using TTX.App.Interfaces.Platforms;

namespace TTX.Tests.App.Factories;

public class PlatformUserFactory(Faker _faker, SlugFactory _slugFactory)
{
    public PlatformUser Create()
    {
        return new PlatformUser()
        {
            Id = _faker.Random.Int(0).ToString(),
            AvatarUrl = new Uri(_faker.Internet.Avatar()),
            DisplayName = _faker.Name.FirstName(),
            Username = _slugFactory.Create()
        };
    }
}
