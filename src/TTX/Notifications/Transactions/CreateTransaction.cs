using TTX.Dto.Transactions;
using TTX.Models;

namespace TTX.Notifications.Transactions
{
    public class CreateTransaction(Transaction tx) : CreatorTransactionDto(tx), INotification;
}