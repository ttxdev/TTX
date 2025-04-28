namespace TTX.Exceptions
{
    public class ZeroAmountException() : DomainException("Amount provided can't be zero or below");
}