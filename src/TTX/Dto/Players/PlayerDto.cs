using System.Text.Json.Serialization;
using TTX.Dto.LootBoxes;
using TTX.Dto.Transactions;
using TTX.Models;

namespace TTX.Dto.Players
{
    public class PlayerDto : PlayerPartialDto
    {
        [JsonPropertyName("transactions")] public required PlayerTransactionDto[] Transactions { get; init; }

        [JsonPropertyName("loot_boxes")] public required LootBoxDto[] LootBoxes { get; init; }

        [JsonPropertyName("shares")] public required PlayerShareDto[] Shares { get; init; }

        [JsonPropertyName("history")] public required PortfolioSnapshotDto[] History { get; init; }

        public static new PlayerDto Create(Player player)
        {
            return new PlayerDto
            {
                Id = player.Id,
                Name = player.Name,
                Slug = player.Slug,
                TwitchId = player.TwitchId,
                Value = player.Value,
                Portfolio = player.Portfolio,
                Credits = player.Credits,
                Type = player.Type,
                AvatarUrl = player.AvatarUrl.ToString(),
                History = player.History.Select(PortfolioSnapshotDto.Create).ToArray(),
                Transactions = player.Transactions.Select(PlayerTransactionDto.Create).ToArray(),
                LootBoxes = player.LootBoxes.Select(LootBoxDto.Create).ToArray(),
                Shares = player.GetShares().Select(PlayerShareDto.Create).ToArray(),
                CreatedAt = player.CreatedAt,
                UpdatedAt = player.UpdatedAt
            };
        }
    }
}