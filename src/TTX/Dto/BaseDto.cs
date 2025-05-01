using System.Text.Json.Serialization;
using TTX.Models;

namespace TTX.Dto
{
    public abstract class BaseDto(Model entity)
    {
        [JsonPropertyName("id")]
        [JsonPropertyOrder(0)]
        public int Id { get; } = entity.Id;

        [JsonPropertyName("created_at")]
        [JsonPropertyOrder(90)]
        public DateTime CreatedAt { get; } = entity.CreatedAt;

        [JsonPropertyName("updated_at")]
        [JsonPropertyOrder(91)]
        public DateTime UpdatedAt { get; } = entity.UpdatedAt;
    }
}