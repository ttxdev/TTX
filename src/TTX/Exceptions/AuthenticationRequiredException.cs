namespace TTX.Exceptions;

public class AuthenticationRequiredException : DomainException
{
    public AuthenticationRequiredException() : base("Authentication required") { }
}