using System.Text.Json.Serialization;
using MediatR;
using TTX.Dto.CreatorApplications;
using TTX.Models;

namespace TTX.Commands.CreatorApplications.UpdateCreatorApplication
{
    public readonly struct UpdateCreatorApplicationCommand : IRequest<CreatorApplicationDto>
    {
        [JsonPropertyName("application_id")] public required int ApplicationId { get; init; }
        [JsonPropertyName("status")] public required CreatorApplicationStatus Status { get; init; }
    }
}