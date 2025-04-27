using TTX.Exceptions;
using TTX.ValueObjects;

namespace TTX.Tests.ValueObjects;

[TestClass]
public class TwitchIdTests
{
    [TestMethod]
    public void Create_WithValidTwitchId_ShouldReturnTwitchIdInstance()
    {
        string validTwitchId = "12345";

        var twitchId = TwitchId.Create(validTwitchId);

        Assert.IsNotNull(twitchId);
        Assert.AreEqual(validTwitchId, twitchId.Value);
    }

    [TestMethod]
    public void Create_WithNullOrEmptyTwitchId_ShouldThrowException()
    {
        string invalidTwitchId = "";

        Assert.ThrowsException<InvalidValueObjectException>(() => TwitchId.Create(invalidTwitchId));
    }

    [TestMethod]
    public void Create_WithInvalidCharacters_ShouldThrowException()
    {
        string invalidTwitchId = "abc123";

        Assert.ThrowsException<InvalidValueObjectException>(() => TwitchId.Create(invalidTwitchId));
    }

    [TestMethod]
    public void ImplicitConversion_FromString_ShouldReturnTwitchIdInstance()
    {
        string validTwitchId = "67890";

        TwitchId twitchId = validTwitchId;

        Assert.AreEqual(validTwitchId, twitchId.Value);
    }

    [TestMethod]
    public void ImplicitConversion_FromInt_ShouldReturnTwitchIdInstance()
    {
        int validTwitchId = 12345;

        TwitchId twitchId = validTwitchId;

        Assert.AreEqual(validTwitchId.ToString(), twitchId.Value);
    }

    [TestMethod]
    public void ImplicitConversion_ToInt_ShouldReturnIntegerValue()
    {
        string validTwitchId = "54321";
        var twitchId = TwitchId.Create(validTwitchId);

        int intValue = twitchId;

        Assert.AreEqual(54321, intValue);
    }
}

