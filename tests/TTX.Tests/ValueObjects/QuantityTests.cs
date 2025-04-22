using TTX.Exceptions;
using TTX.ValueObjects;

namespace TTX.Tests.ValueObjects;

[TestClass]
public class QuantityTests
{
    [TestMethod]
    public void Create_WithValidValue_ShouldReturnQuantityInstance()
    {
        int validValue = 10;

        var quantity = Quantity.Create(validValue);

        Assert.IsNotNull(quantity);
        Assert.AreEqual(validValue, quantity.Value);
    }

    [TestMethod]
    public void Create_WithNegativeValue_ShouldThrowException()
    {
        int negativeValue = -1;

        Assert.ThrowsException<InvalidValueObjectException>(() => Quantity.Create(negativeValue));
    }

    [TestMethod]
    public void ImplicitConversion_FromInt_ShouldReturnQuantityInstance()
    {
        int value = 5;

        Quantity quantity = value;

        Assert.AreEqual(value, quantity.Value);
    }
}
