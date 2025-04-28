namespace TTX.Exceptions
{
    public class ExceedsBalanceException : DomainException
    {
        public ExceedsBalanceException() : base("Not enough credits")
        {
        }
    }
}