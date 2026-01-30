using Bogus;
using TTX.Domain.ValueObjects;

namespace TTX.Tests.App.Factories;

public class NameFactory(Faker faker)
{
    public Name Create()
    {
        string name = faker.Name.FirstName();
        if (name.Length < Name.MinLength)
        {
            name = name.PadRight(Name.MinLength, 'a');
        }

        return name;
    }
}
