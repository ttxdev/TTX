using System.Text.Json.Serialization;
using TTX.App.Dto.Transactions;
using TTX.Domain.Models;

namespace TTX.App.Dto.Creators;

public class CreatorDto : CreatorPartialDto
{
    [JsonPropertyName("transactions")] public required CreatorTransactionDto[] Transactions { get; init; }

    [JsonPropertyName("shares")] public required CreatorShareDto[] Shares { get; init; }

    public static new CreatorDto Create(Creator creator)
    {
        return new CreatorDto
        {
            Id = creator.Id,
            Name = creator.Name,
            Slug = creator.Slug,
            Ticker = creator.Ticker,
            Platform = creator.Platform,
            PlatformId = creator.PlatformId,
            Value = creator.Value,
            AvatarUrl = creator.AvatarUrl.ToString(),
            StreamStatus = StreamStatusDto.Create(creator.StreamStatus),
            Transactions = creator.Transactions.Select(CreatorTransactionDto.Create).ToArray(),
            Shares = creator.GetShares().Select(CreatorShareDto.Create).ToArray(),
            History = creator.History.Select(VoteDto.Create).ToArray(),
            CreatedAt = creator.CreatedAt,
            UpdatedAt = creator.UpdatedAt
        };
    }
}
