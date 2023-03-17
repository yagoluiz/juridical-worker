using System.Text.Json;
using System.Text.Json.Serialization;

namespace Juridical.Core.Models.Publishers;

public class Message
{
    private Message(string type, string payload)
    {
        Guid.NewGuid();
        OrderingKey = Guid.NewGuid();
        Type = type;
        Payload = payload;
    }

    [JsonPropertyName("orderingKey")] public Guid OrderingKey { get; }
    [JsonPropertyName("type")] public string Type { get; }
    [JsonPropertyName("payload")] public string Payload { get; }

    public static Message FromEvent(object @event)
        => new(@event.GetType().FullName!, JsonSerializer.Serialize(@event));
}
