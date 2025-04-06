namespace TTX.Core.Exceptions;

public class MaxSharesException(int max) : DomainException($"Cannot own more than {max} shares of a creator")
{
}