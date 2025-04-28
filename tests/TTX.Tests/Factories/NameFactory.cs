using System.Text.RegularExpressions;
using Bogus;
using TTX.ValueObjects;

namespace TTX.Tests.Factories;

public partial class NameFactory
{
    public static Name Create()
    {
        Faker faker = new();
        var first = faker.Name.FirstName();
        var suffix = faker.Random.Int(1, 100000);

        return IllegalCharacters().Replace($"{first}_{suffix}".ToLower(), "");
    }

    [GeneratedRegex(@"[^A-Za-z0-9_]")]
    private static partial Regex IllegalCharacters();
}