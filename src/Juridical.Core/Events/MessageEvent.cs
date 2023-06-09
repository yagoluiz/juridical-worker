using System.Text.Json.Serialization;

namespace Juridical.Core.Events;

public record MessageEvent([property: JsonPropertyName("data")] string Data);
