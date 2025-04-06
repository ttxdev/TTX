namespace TTX.Core.Exceptions;

public class ExceedsSharesException : DomainException
{
    public ExceedsSharesException() : base("Not enough shares")
    { }
}