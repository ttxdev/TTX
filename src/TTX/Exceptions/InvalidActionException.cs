namespace TTX.Exceptions
{
    public class InvalidActionException : DomainException
    {
        public InvalidActionException(string message) : base(message)
        {
        }
    }
}
