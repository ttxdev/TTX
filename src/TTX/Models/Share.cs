using TTX.ValueObjects;

namespace TTX.Models
{
    public class Share
    {
        public required Creator Creator { get; init; }
        public required Player Player { get; init; }
        public Quantity Quantity { get; private set; } = 0;

        public void Count(Transaction tx)
        {
            if (tx.IsBuy())
            {
                Quantity += tx.Quantity;
            }
            else
            {
                Quantity -= tx.Quantity;
            }
        }
    }
}