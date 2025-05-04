using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Commands.Ordering.PlaceOrder
{
    public readonly struct PlaceOrderCommand : ICommand<Transaction>
    {
        public required ModelId Actor { get; init; }
        public required Slug Creator { get; init; }
        public required Quantity Amount { get; init; }
        public required TransactionAction Action { get; init; }
    }
}
