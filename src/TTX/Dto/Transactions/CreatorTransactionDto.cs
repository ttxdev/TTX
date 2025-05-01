using System.Text.Json.Serialization;
using TTX.Dto.Players;
using TTX.Models;

namespace TTX.Dto.Transactions
{
    public class CreatorTransactionDto : TransactionDto
    {
        [JsonPropertyName("player")] public required PlayerPartialDto Player { get; init; }

        public static CreatorTransactionDto Create(Transaction tx)
        {
            return new CreatorTransactionDto
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