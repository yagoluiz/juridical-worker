using System.Text.Json.Serialization;

namespace Juridical.Worker.Models.Requests;

public record MessageContentRequest(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("type")] string Type = "text");
