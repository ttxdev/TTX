namespace TTX.Exceptions
{
    public class NoLootBoxesException : DomainException
    {
        public NoLootBoxesException() : base("No loot boxes available") { }
    }
}