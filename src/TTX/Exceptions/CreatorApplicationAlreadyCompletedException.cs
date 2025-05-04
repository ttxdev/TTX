namespace TTX.Exceptions
{
    public class CreatorApplicationAlreadyCompletedException : DomainException
    {
        public CreatorApplicationAlreadyCompletedException() : base("This application has already completed. You can't change this you flippin donut.") { }
    }
}
