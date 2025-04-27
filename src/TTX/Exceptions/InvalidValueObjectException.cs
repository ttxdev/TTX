namespace TTX.Exceptions;

public class InvalidValueObjectException(string obj, string message) : DomainException($"{obj} {message}");
