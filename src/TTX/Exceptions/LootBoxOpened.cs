namespace TTX.Exceptions
{
    public class LootBoxOpenedException : DomainException
    {
        public LootBoxOpenedException() : base("Loot box already opened") { }
    }
}