using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Commands.Players.CalculatePortfolio
{
    public readonly struct CalculatePortfolioCommand : ICommand<PortfolioSnapshot>
    {
        public required ModelId PlayerId { get; init; }
    }
}