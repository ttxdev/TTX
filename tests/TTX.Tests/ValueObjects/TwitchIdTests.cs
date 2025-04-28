using TTX.Exceptions;
using TTX.ValueObjects;

namespace TTX.Tests.ValueObjects;

[TestClass]
public class TwitchIdTests
{
    [TestMethod]
    public void Create_WithValidTwitchId_ShouldReturnTwitchIdInstance()
    {
        var validTwitchId = "12345";

        var twitchId = TwitchId.Create(validTwitchId);

        Assert.IsNotNull(twitchId);
        Assert.AreEqual(validTwitchId, twitchId.Value);
    }

    [TestMethod]
    public void Create_WithNullOrEmptyTwitchId_ShouldThrowException()
    {
        var invalidTwitchId = "";

        Assert.ThrowsException<InvalidValueObjectException>(() => TwitchId.Create(invalidTwitchId));
    }

    [TestMethod]
    public void Create_WithInvalidCharacters_ShouldThrowException()
    {
        var invalidTwitchId = "abc123";

        Assert.ThrowsException<InvalidValueObjectException>(() => TwitchId.Create(invalidTwitchId));
    }

    [TestMethod]
    public void ImplicitConversion_FromString_ShouldReturnTwitchIdInstance()
    {
        var validTwitchId = "67890";

        TwitchId twitchId = validTwitchId;

        Assert.AreEqual(validTwitchId, twitchId.Value);
    }

    [TestMethod]
    public void ImplicitConversion_FromInt_ShouldReturnTwitchIdInstance()
    {
        var validTwitchId = 12345;

        TwitchId twitchId = validTwitchId;

        Assert.AreEqual(validTwitchId.ToString(), twitchId.Value);
    }

    [TestMethod]
    public void ImplicitConversion_ToInt_ShouldReturnIntegerValue()
    {
        var validTwitchId = "54321";
        var twitchId = TwitchId.Create(validTwitchId);

        int intValue = twitchId;

        Assert.AreEqual(54321, intValue);
    }
}