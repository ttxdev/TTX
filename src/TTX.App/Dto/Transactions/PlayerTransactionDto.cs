using System.Text.Json.Serialization;
using TTX.App.Dto.Creators;
using TTX.Domain.Models;

namespace TTX.App.Dto.Transactions;

public class PlayerTransactionDto : TransactionDto
{
    [JsonPropertyName("creator")] public required CreatorPartialDto Creator { get; init; }

    public static PlayerTransactionDto Create(Transaction tx)
    {
        return new PlayerTransactionDto
        {
            Id = tx.Id,
            Action = tx.Action,
            Value = tx.Value,
            Quantity = tx.Quantity,
            Creator = CreatorPartialDto.Create(tx.Creator),
            CreatorId = tx.CreatorId,
            PlayerId = tx.PlayerId,
            CreatedAt = tx.CreatedAt,
            UpdatedAt = tx.UpdatedAt
        };
    }
}
