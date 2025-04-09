namespace TTX.Core.Exceptions;

public class DiscordUserNotFoundException : DomainException
{
    public DiscordUserNotFoundException() : base($"Discord user not found") { }
}