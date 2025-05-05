using System.Text.Json.Serialization;
using MediatR;
using TTX.Models;

namespace TTX.Commands.CreatorApplications.UpdateCreatorApplication
{
    public readonly struct UpdateCreatorApplicationCommand : IRequest<CreatorApplication>
    {
        [JsonPropertyName("application_id")] public required int ApplicationId { get; init; }
        [JsonPropertyName("status")] public required CreatorApplicationStatus Status { get; init; }
    }
}