namespace TTX.Exceptions
{
    public class NotFoundException(string message) : DomainException(message);
}