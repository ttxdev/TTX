using System.Text.Json.Serialization;
using TTX.Core.Models;

namespace TTX.Interface.Api.Dto;

public abstract class BaseDto<T>(T entity) where T : ModelBase
{
    [JsonPropertyName("id")]
    public int Id { get; } = entity.Id;
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; } = entity.CreatedAt;
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; } = entity.UpdatedAt;
}