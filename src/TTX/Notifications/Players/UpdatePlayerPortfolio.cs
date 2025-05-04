using TTX.Dto.Portfolios;
using TTX.Models;

namespace TTX.Notifications.Players
{
    public class UpdatePlayerPortfolio : PortfolioDto, INotification
    {
        public static new UpdatePlayerPortfolio Create(PortfolioSnapshot p)
        {
            return new UpdatePlayerPortfolio
            {
                PlayerId= p.PlayerId, 
                Value = p.Value, 
                Time = p.Time
            };
        }
    }
}