namespace TTX.Core.Exceptions;

public class ExceedsBalanceException : DomainException
{
    public ExceedsBalanceException() : base("Not enough credits")
    { }
}