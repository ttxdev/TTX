using TTX.Exceptions;
using TTX.ValueObjects;

namespace TTX.Tests.ValueObjects;

[TestClass]
public class CreditsTests
{
    [TestMethod]
    public void Create_WithValidValue_ShouldReturnCreditsInstance()
    {
        long validValue = 100;

        var credits = Credits.Create(validValue);

        Assert.IsNotNull(credits);
        Assert.AreEqual(validValue, credits.Value);
    }

    [TestMethod]
    public void Create_WithNegativeValue_ShouldThrowException()
    {
        long negativeValue = -1;

        Assert.ThrowsException<InvalidValueObjectException>(() => { Credits.Create(negativeValue); });
    }

    [TestMethod]
    public void ImplicitConversion_FromLong_ShouldReturnCreditsInstance()
    {
        long value = 50;
        Credits credits = value;

        Assert.AreEqual(value, credits.Value);
    }

    [TestMethod]
    public void ImplicitConversion_FromInt_ShouldReturnCreditsInstance()
    {
        var value = 25;
        Credits credits = value;

        Assert.AreEqual(value, credits.Value);
    }

    [TestMethod]
    public void ToString_ShouldReturnValueAsString()
    {
        long value = 75;
        var credits = Credits.Create(value);

        var result = credits.ToString();

        Assert.AreEqual(value.ToString(), result);
    }

    [TestMethod]
    public void GetHashCode_ShouldReturnHashCodeOfValue()
    {
        long value = 100;
        var credits = Credits.Create(value);

        var hashCode = credits.GetHashCode();

        Assert.AreEqual(value.GetHashCode(), hashCode);
    }
}