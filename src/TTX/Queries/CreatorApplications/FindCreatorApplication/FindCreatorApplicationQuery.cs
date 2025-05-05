using System.Text.Json.Serialization;
using TTX.Dto.CreatorApplications;

namespace TTX.Queries.CreatorApplications.FindCreatorApplication
{
    public readonly struct FindCreatorApplicationQuery : IQuery<CreatorApplicationDto?>
    {
        [JsonPropertyName("id")] public required int Id { get; init; }
    }
}