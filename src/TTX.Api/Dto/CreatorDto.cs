using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Api.Dto;

public class CreatorDto(Creator creator) : CreatorPartialDto(creator)
{
    [JsonPropertyName("transactions")]
    [JsonPropertyOrder(17)]
    public CreatorTransactionDto[] Transactions { get; } = [.. creator.Transactions.Select(x => new CreatorTransactionDto(x))];
    [JsonPropertyName("shares")]
    [JsonPropertyOrder(18)]
    public CreatorShareDto[] Shares { get; } = [.. creator.GetShares().Select(x => new CreatorShareDto(x))];
}