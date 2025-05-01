using System.Text.Json.Serialization;
using TTX.Dto.LootBoxes;
using TTX.Dto.Transactions;
using TTX.Models;

namespace TTX.Dto.Players
{
    public class PlayerDto(Player player) : PlayerPartialDto(player)
    {
        [JsonPropertyName("transactions")]
        public PlayerTransactionDto[] Transactions { get; } =
            [.. player.Transactions.Take(20).Select(x => new PlayerTransactionDto(x))];

        [JsonPropertyName("loot_boxes")]
        public LootBoxDto[] LootBoxes { get; } = [.. player.LootBoxes.Select(x => new LootBoxDto(x))];

        [JsonPropertyName("shares")]
        public PlayerShareDto[] Shares { get; } = [.. player.GetShares().Select(x => new PlayerShareDto(x))];
    }
}