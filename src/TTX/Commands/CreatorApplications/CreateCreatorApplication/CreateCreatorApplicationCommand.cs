using System.Text.Json.Serialization;
using MediatR;
using TTX.Models;

namespace TTX.Commands.CreatorApplications.CreateCreatorApplication
{
    public readonly struct CreateCreatorApplicationCommand : IRequest<CreatorApplication>
    {
        [JsonPropertyName("submitter_id")] public required int SubmitterId { get; init; }
        [JsonPropertyName("username")] public required string Username { get; init; }
        [JsonPropertyName("ticker")] public required string Ticker { get; init; }
    }
}