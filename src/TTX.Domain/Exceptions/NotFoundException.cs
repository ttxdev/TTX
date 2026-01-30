namespace TTX.Domain.Exceptions;

public class NotFoundException<T>() : TtxException($"{nameof(T)} not found");
