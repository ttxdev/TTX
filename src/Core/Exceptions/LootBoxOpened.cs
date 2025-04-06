namespace TTX.Core.Exceptions;

public class LootBoxOpenedException : DomainException
{
    public LootBoxOpenedException() : base("Loot box already opened") { }
}