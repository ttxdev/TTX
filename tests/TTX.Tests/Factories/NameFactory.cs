using System.Text.RegularExpressions;
using Bogus;
using TTX.ValueObjects;

namespace TTX.Tests.Factories;

public partial class NameFactory
{
    public static Name Create()
    {
        Faker faker = new();
        string first = faker.Name.FirstName();
        string last = faker.Name.LastName();

        return IllegalCharacters().Replace($"{first}_{last}".ToLower(), "");
    }

    [GeneratedRegex(@"[^A-Za-z0-9_]")]
    private static partial Regex IllegalCharacters();
}
