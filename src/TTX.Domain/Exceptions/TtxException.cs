namespace TTX.Domain.Exceptions;

[Serializable]
public class TtxException : Exception
{
    public TtxException() { }
    public TtxException(string message) : base(message) { }
    public TtxException(string message, Exception inner) : base(message, inner) { }
}
