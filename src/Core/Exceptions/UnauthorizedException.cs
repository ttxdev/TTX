namespace TTX.Core.Exceptions;

public class UnauthorizedException : DomainException
{
    public UnauthorizedException() : base("Unauthorized") { }
}