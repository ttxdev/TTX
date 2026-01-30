using System.Text.Json.Serialization;

namespace TTX.App.Events;

public record BaseEvent
{
    [JsonPropertyName("type")]
    public string Type => GetType().Name;
}
