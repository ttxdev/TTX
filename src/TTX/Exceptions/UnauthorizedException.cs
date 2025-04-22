namespace TTX.Exceptions;

public class UnauthorizedException : DomainException
{
    public UnauthorizedException() : base("Unauthorized") { }
}