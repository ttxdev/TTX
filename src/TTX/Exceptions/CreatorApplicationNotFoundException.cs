namespace TTX.Exceptions
{
    public class CreatorApplicationNotFoundException : NotFoundException
    {
        public CreatorApplicationNotFoundException() : base("This application does not exist.") { }
    }
}
