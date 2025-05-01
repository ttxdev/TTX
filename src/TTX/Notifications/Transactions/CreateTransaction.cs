using TTX.Dto.Players;
using TTX.Dto.Transactions;
using TTX.Models;

namespace TTX.Notifications.Transactions
{
    public class CreateTransaction : CreatorTransactionDto, INotification
    {
        public static new CreateTransaction Create(Transaction tx)
        {
            return new CreateTransaction
            {
                Id = tx.Id,
                Action = tx.Action,
                Value = tx.Value,
                Quantity = tx.Quantity,
                CreatorId = tx.CreatorId,
                PlayerId = tx.PlayerId,
                Player = PlayerPartialDto.Create(tx.Player),
                CreatedAt = tx.CreatedAt,
                UpdatedAt = tx.UpdatedAt
            };
        }
    }
}