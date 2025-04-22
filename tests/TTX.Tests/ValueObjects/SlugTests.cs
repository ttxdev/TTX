using TTX.Exceptions;
using TTX.ValueObjects;

namespace TTX.Tests.ValueObjects;

[TestClass]
public class SlugTests
{
    [TestMethod]
    public void Create_WithValidSlug_ShouldReturnSlugInstance()
    {
        string validSlug = "valid_slug";

        var slug = Slug.Create(validSlug);

        Assert.AreEqual(validSlug, slug.Value);
    }

    [TestMethod]
    public void Create_WithNullOrEmptySlug_ShouldThrowException()
    {
        string invalidSlug = "";

        Assert.ThrowsException<InvalidValueObjectException>(() => Slug.Create(invalidSlug));
    }

    [TestMethod]
    public void Create_WithSlugTooShort_ShouldThrowException()
    {
        string shortSlug = "ab";

        Assert.ThrowsException<InvalidValueObjectException>(() => Slug.Create(shortSlug));
    }

    [TestMethod]
    public void Create_WithSlugTooLong_ShouldThrowException()
    {
        string longSlug = new('a', Slug.MaxLength + 1);

        Assert.ThrowsException<InvalidValueObjectException>(() => Slug.Create(longSlug));
    }

    [TestMethod]
    public void Create_WithInvalidCharacters_ShouldThrowException()
    {
        string invalidSlug = "Invalid@Slug!";

        Assert.ThrowsException<InvalidValueObjectException>(() => Slug.Create(invalidSlug));
    }

    [TestMethod]
    public void Create_WithUppercaseCharacters_ShouldConvertToLowercase()
    {
        string uppercaseSlug = "UPPERCASE_SLUG";

        var slug = Slug.Create(uppercaseSlug);

        Assert.AreEqual(uppercaseSlug.ToLower(), slug.Value);
    }

    [TestMethod]
    public void ImplicitConversion_FromString_ShouldReturnSlugInstance()
    {
        string validSlug = "another_valid_slug";

        Slug slug = validSlug;

        Assert.AreEqual(validSlug, slug.Value);
    }

    [TestMethod]
    public void ImplicitConversion_FromName_ShouldReturnSlugInstance()
    {
        string nameValue = "ValidName";
        var name = Name.Create(nameValue);

        Slug slug = name;

        Assert.AreEqual(nameValue.ToLower(), slug.Value);
    }
}
