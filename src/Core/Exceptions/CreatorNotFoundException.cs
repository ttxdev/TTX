namespace TTX.Core.Exceptions;

public class CreatorNotFoundException : DomainException
{
    public CreatorNotFoundException() : base($"Creator not found") { }
}