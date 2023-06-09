using System.Text.Json.Serialization;

namespace Juridical.Core.Events;

public record LegalProcessEvent([property: JsonPropertyName("processCount")] int ProcessCount);
