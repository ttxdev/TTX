using TTX.Exceptions;
using TTX.ValueObjects;

namespace TTX.Tests.ValueObjects;

[TestClass]
public class SlugTests
{
    [TestMethod]
    public void Create_WithValidSlug_ShouldReturnSlugInstance()
    {
        var validSlug = "valid_slug";

        var slug = Slug.Create(validSlug);

        Assert.AreEqual(validSlug, slug.Value);
    }

    [TestMethod]
    public void Create_WithNullOrEmptySlug_ShouldThrowException()
    {
        var invalidSlug = "";

        Assert.ThrowsException<InvalidValueObjectException>(() => Slug.Create(invalidSlug));
    }

    [TestMethod]
    public void Create_WithSlugTooShort_ShouldThrowException()
    {
        var shortSlug = "ab";

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
        var invalidSlug = "Invalid@Slug!";

        Assert.ThrowsException<InvalidValueObjectException>(() => Slug.Create(invalidSlug));
    }

    [TestMethod]
    public void Create_WithUppercaseCharacters_ShouldConvertToLowercase()
    {
        var uppercaseSlug = "UPPERCASE_SLUG";

        var slug = Slug.Create(uppercaseSlug);

        Assert.AreEqual(uppercaseSlug.ToLower(), slug.Value);
    }

    [TestMethod]
    public void ImplicitConversion_FromString_ShouldReturnSlugInstance()
    {
        var validSlug = "another_valid_slug";

        Slug slug = validSlug;

        Assert.AreEqual(validSlug, slug.Value);
    }

    [TestMethod]
    public void ImplicitConversion_FromName_ShouldReturnSlugInstance()
    {
        var nameValue = "ValidName";
        var name = Name.Create(nameValue);

        Slug slug = name;

        Assert.AreEqual(nameValue.ToLower(), slug.Value);
    }
}