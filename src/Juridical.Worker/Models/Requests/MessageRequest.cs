using System.Text.Json.Serialization;

namespace Juridical.Worker.Models.Requests;

public record MessageRequest(
    [property: JsonPropertyName("from")] string From,
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("contents")]
    IList<MessageContentRequest> Contents);
