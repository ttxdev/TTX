namespace TTX.Core.Exceptions;

public class ZeroAmountException : DomainException
{
    public ZeroAmountException() : base("Amount provided can't be zero or below")
    { }
}