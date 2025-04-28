using TTX.Exceptions;
using TTX.ValueObjects;

namespace TTX.Tests.ValueObjects;

[TestClass]
public class TickerTests
{
    [TestMethod]
    public void Create_WithValidTicker_ShouldReturnTickerInstance()
    {
        var validTicker = "AAPL";

        var ticker = Ticker.Create(validTicker);

        Assert.IsNotNull(ticker);
        Assert.AreEqual(validTicker, ticker.Value);
    }

    [TestMethod]
    public void Create_WithNullOrEmptyTicker_ShouldThrowException()
    {
        var invalidTicker = "";

        Assert.ThrowsException<InvalidValueObjectException>(() => Ticker.Create(invalidTicker));
    }

    [TestMethod]
    public void Create_WithTickerTooShort_ShouldThrowException()
    {
        var shortTicker = "A";

        Assert.ThrowsException<InvalidValueObjectException>(() => Ticker.Create(shortTicker));
    }

    [TestMethod]
    public void Create_WithTickerTooLong_ShouldThrowException()
    {
        var longTicker = "ABCDEFGHIJKLMNO1";

        Assert.ThrowsException<InvalidValueObjectException>(() => Ticker.Create(longTicker));
    }

    [TestMethod]
    public void Create_WithInvalidCharacters_ShouldThrowException()
    {
        var invalidTicker = "AAPL@";

        Assert.ThrowsException<InvalidValueObjectException>(() => Ticker.Create(invalidTicker));
    }

    [TestMethod]
    public void ImplicitConversion_FromString_ShouldReturnTickerInstance()
    {
        var validTicker = "MSFT";

        Ticker ticker = validTicker;

        Assert.AreEqual(validTicker, ticker.Value);
    }
}