using System.Text.Json.Serialization;

namespace Juridical.Worker.Models.Responses;

public record MessageResponse([property: JsonPropertyName("id")] string Id);
