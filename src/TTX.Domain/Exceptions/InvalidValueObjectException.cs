namespace TTX.Domain.Exceptions;

public class InvalidValueObjectException<T>(string message) : TtxException($"{nameof(T)} {message}");
