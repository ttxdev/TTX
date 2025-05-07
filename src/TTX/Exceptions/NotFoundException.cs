namespace TTX.Exceptions
{
    public class NotFoundException<T>() : DomainException($"{nameof(T)} not found");
}