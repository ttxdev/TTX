using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Commands.Players.CalculatePortfolio
{
    public readonly struct CalculatePortfolioCommand : ICommand<PortfolioSnapshot>
    {
        [JsonPropertyName("player_id")] public required int PlayerId { get; init; }
    }
}