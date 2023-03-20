using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Http;
using CloudNative.CloudEvents.SystemTextJson;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Juridical.Core.Interfaces;
using Juridical.Core.Models.Publishers;
using Juridical.Core.Parses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Juridical.Core.Services;

public class GooglePubSubPublisherService : IPublisherService
{
    private readonly PublisherClient _publisherClient;
    private readonly ILogger<GooglePubSubPublisherService> _logger;

    public GooglePubSubPublisherService(
        IConfiguration configuration,
        ILogger<GooglePubSubPublisherService> logger)
    {
        _logger = logger;

        _logger.LogInformation("GooglePubSubPublisherService - ProjectId: {projectId} - Topic: {topic}",
            configuration["PROJECT_ID"], configuration["LEGAL_PROCESS_TOPIC_ID"]);

        _publisherClient = new PublisherClientBuilder
        {
            TopicName =
                TopicName.FromProjectTopic(configuration["PROJECT_ID"], configuration["LEGAL_PROCESS_TOPIC_ID"]),
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
            Settings = new PublisherClient.Settings { EnableMessageOrdering = true }
        }.Build();
    }

    public async Task PublishAsync(Message message)
    {
        _logger.LogInformation("GooglePubSubPublisherService - Message: {message}", message.Payload);

        var cloudEventMessage = GooglePubSubMessageCloudEventParse.CreateLegalProcessEvent(message);
        var cloudEventContent = cloudEventMessage.ToHttpContent(ContentMode.Structured, new JsonEventFormatter());
        var cloudEvent = await cloudEventContent.ReadAsStringAsync();

        var pubSubMessage = new PubsubMessage
        {
            OrderingKey = message.OrderingKey.ToString(),
            Attributes =
            {
                { "eventType", cloudEventMessage.Type },
                { "eventSource", cloudEventMessage.Source!.ToString() }
            },
            Data = ByteString.CopyFromUtf8(cloudEvent)
        };

        await _publisherClient.PublishAsync(pubSubMessage);
    }
}
