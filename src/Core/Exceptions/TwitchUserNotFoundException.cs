namespace TTX.Core.Exceptions;

public class TwitchUserNotFoundException : DomainException
{
    public TwitchUserNotFoundException() : base($"Creator not found") { }
}