using System.Text.Json.Serialization;

namespace Juridical.Core.Models.Responses;

public record MessageResponse([property: JsonPropertyName("id")] string Id);
