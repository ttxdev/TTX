using System.Text.Json.Serialization;
using TTX.Core.Models;

namespace TTX.Interface.Api.Dto;

public class UserDto(User user) : UserPartialDto(user)
{
    [JsonPropertyName("transactions")]
    public UserTransactionDto[] Transactions { get; } = [.. user.Transactions.Take(20).Select(x => new UserTransactionDto(x))];
    [JsonPropertyName("loot_boxes")]
    public LootBoxDto[] LootBoxes { get; } = [.. user.LootBoxes.Select(x => new LootBoxDto(x))];
    [JsonPropertyName("shares")]
    public UserShareDto[] Shares { get; } = [.. user.GetShares().Select(x => new UserShareDto(x))];
}