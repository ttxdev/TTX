namespace TTX.Core;

[System.Serializable]
public class DomainException : Exception
{
    public DomainException() { }
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception inner) : base(message, inner) { }
    protected DomainException(
      System.Runtime.Serialization.SerializationInfo info,
#pragma warning disable SYSLIB0051 // Type or member is obsolete
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#pragma warning restore SYSLIB0051 // Type or member is obsolete
}