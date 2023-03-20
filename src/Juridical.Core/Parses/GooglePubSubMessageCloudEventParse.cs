using CloudNative.CloudEvents;
using Juridical.Core.Models.Publishers;

namespace Juridical.Core.Parses;

public static class GooglePubSubMessageCloudEventParse
{
    public static CloudEvent CreateLegalProcessEvent(Message message)
        => new()
        {
            Id = Guid.NewGuid().ToString(),
            Type = message.Type,
            Source = new Uri("juridical-legal-process-worker", UriKind.Relative),
            DataContentType = "application/json",
            Time = DateTimeOffset.Now,
            Data = message.Payload
        };
}
