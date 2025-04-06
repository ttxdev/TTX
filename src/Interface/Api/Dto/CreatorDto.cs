using System.Text.Json.Serialization;
using TTX.Core.Models;

namespace TTX.Interface.Api.Dto;

public class CreatorDto(Creator creator) : CreatorPartialDto(creator)
{
    [JsonPropertyName("transactions")]
    public CreatorTransactionDto[] Transactions { get; } = [.. creator.Transactions.Select(x => new CreatorTransactionDto(x))];
    [JsonPropertyName("shares")]
    public UserShareDto[] Shares { get; } = [.. creator.GetShares().Select(x => new UserShareDto(x))];
}