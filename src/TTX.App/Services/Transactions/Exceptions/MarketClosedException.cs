using TTX.Domain.Exceptions;

namespace TTX.App.Services.Transactions.Exceptions;

public class MarketClosedException : InvalidActionException
{
    public MarketClosedException() : base("Market is closed")
    {
    }
}
