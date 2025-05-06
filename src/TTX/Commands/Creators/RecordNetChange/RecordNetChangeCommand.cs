using System.Text.Json.Serialization;
using TTX.Dto.Creators;
using TTX.Models;

namespace TTX.Commands.Creators.RecordNetChange
{
    public readonly struct RecordNetChangeCommand : ICommand<VoteDto>
    {
        [JsonPropertyName("username")] public required string Username { get; init; }
        [JsonPropertyName("net_change")] public required int NetChange { get; init; }
    }
}