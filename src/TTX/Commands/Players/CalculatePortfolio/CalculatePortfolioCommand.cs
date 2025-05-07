using System.Text.Json.Serialization;
using TTX.Dto.Players;
using TTX.Models;

namespace TTX.Commands.Players.CalculatePortfolio
{
    public readonly struct CalculatePortfolioCommand : ICommand<PortfolioSnapshotDto>
    {
        [JsonPropertyName("player_id")] public required int PlayerId { get; init; }
    }
}