using System.Text.Json.Serialization;
using TTX.Dto.Creators;
using TTX.Models;

namespace TTX.Commands.Creators.UpdateStreamStatus
{
    public readonly struct UpdateStreamStatusCommand : ICommand<StreamStatusDto>
    {
        [JsonPropertyName("username")] public required string Username { get; init; }
        [JsonPropertyName("is_live")] public required bool IsLive { get; init; }
        [JsonPropertyName("at")] public required DateTimeOffset At { get; init; }
    }
}