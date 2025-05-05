using System.Text.Json.Serialization;
using TTX.Dto.Transactions;
using TTX.Models;

namespace TTX.Notifications.Transactions
{
    public class CreateTransaction : INotification
    {
        [JsonPropertyName("transaction")] public required CreatorTransactionDto Transaction { get; init; }

        public static CreateTransaction Create(Transaction tx)
        {
            return new CreateTransaction { Transaction = CreatorTransactionDto.Create(tx) };
        }
    }
}