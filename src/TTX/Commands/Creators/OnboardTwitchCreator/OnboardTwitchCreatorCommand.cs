using System.Text.Json.Serialization;
using MediatR;
using TTX.Dto.Creators;
using TTX.Models;
using TTX.ValueObjects;

namespace TTX.Commands.Creators.OnboardTwitchCreator
{
    public readonly struct OnboardTwitchCreatorCommand : IRequest<CreatorDto>
    {
        [JsonPropertyName("username")] public string? Username { get; init; }
        [JsonPropertyName("twitch_id")] public TwitchId? TwitchId { get; init; }
        [JsonPropertyName("ticker")] public required Ticker Ticker { get; init; }
    }
}