using System.Text.Json.Serialization;

namespace TTX.App.Dto;

public abstract class BaseDto
{
    [JsonPropertyName("id")] public required int Id { get; init; }

    [JsonPropertyName("created_at")] public required DateTime CreatedAt { get; init; }

    [JsonPropertyName("updated_at")] public required DateTime UpdatedAt { get; init; }
}
