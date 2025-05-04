using TTX.Dto.LootBoxes;
using TTX.Dto.Players;
using TTX.Dto.Portfolios;
using TTX.Dto.Transactions;
using TTX.Models;

namespace TTX.Notifications.Players
{
    public class CreatePlayer : PlayerDto, INotification
    {
        public static new CreatePlayer Create(Player player)
        {
            return new CreatePlayer
            {
                Id = player.Id,
                Name = player.Name,
                Slug = player.Slug,
                TwitchId = player.TwitchId,
                Credits = player.Credits,
                Value = player.Value,
                Portfolio = player.Portfolio,
                Type = player.Type,
                AvatarUrl = player.AvatarUrl.ToString(),
                History = player.History.Select(PortfolioDto.Create).ToArray(),
                Transactions = player.Transactions.Select(PlayerTransactionDto.Create).ToArray(),
                LootBoxes = player.LootBoxes.Select(LootBoxDto.Create).ToArray(),
                Shares = player.GetShares().Select(PlayerShareDto.Create).ToArray(),
                CreatedAt = player.CreatedAt,
                UpdatedAt = player.UpdatedAt
            };
        }
    }
}