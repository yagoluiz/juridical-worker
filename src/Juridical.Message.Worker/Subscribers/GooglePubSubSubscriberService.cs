using System.Text.Json;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Juridical.Core.Events;
using Juridical.Core.Interfaces;
using Juridical.Core.Models.Requests;
using Juridical.Core.Models.Responses;

namespace Juridical.Message.Worker.Subscribers;

public sealed class GooglePubSubSubscriberService : ISubscriberService
{
    private readonly SubscriberClient _subscriberClient;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<GooglePubSubSubscriberService> _logger;

    public GooglePubSubSubscriberService(
        IConfiguration configuration,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<GooglePubSubSubscriberService> logger)
    {
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;

        _logger.LogInformation("GooglePubSubSubscriberService - ProjectId: {projectId} - Subscription: {subscription}",
            configuration["PROJECT_ID"], configuration["LEGAL_PROCESS_SUBSCRIPTION_ID"]);

        _subscriberClient = new SubscriberClientBuilder
        {
            SubscriptionName = SubscriptionName.FromProjectSubscription(configuration["PROJECT_ID"],
                configuration["LEGAL_PROCESS_SUBSCRIPTION_ID"]),
            EmulatorDetection = EmulatorDetection.EmulatorOrProduction
        }.Build();
    }

    public async Task SubscriberAsync(CancellationToken cancellationToken)
    {
        var task = _subscriberClient.StartAsync(async (message, _) =>
        {
            try
            {
                _logger.LogInformation("GooglePubSubSubscriberService - MessageId: {messageId} - Message: {message}",
                    message.MessageId, message.Data.ToStringUtf8());

                var messageEvent = JsonSerializer.Deserialize<MessageEvent>(message.Data.ToStringUtf8());
                var legalProcessEvent = JsonSerializer.Deserialize<LegalProcessEvent>(messageEvent!.Data);

                if (legalProcessEvent is null)
                {
                    _logger.LogInformation("GooglePubSubSubscriberService - MessageId: {messageId} - Event: {event}",
                        message.MessageId, legalProcessEvent);

                    return SubscriberClient.Reply.Nack;
                }

                _logger.LogInformation("GooglePubSubSubscriberService - Message count deserialize: {message}",
                    legalProcessEvent.ProcessCount);

                var sendMessage = await SendMessageAsync(legalProcessEvent.ProcessCount);

                if (!sendMessage.Success)
                {
                    _logger.LogCritical("LegalProcessWorker - Error message: {content}", sendMessage.Content);
                    return SubscriberClient.Reply.Nack;
                }

                _logger.LogInformation(
                    "LegalProcessWorker - Success message: {content}", (sendMessage.Content as MessageResponse)?.Id);

                return SubscriberClient.Reply.Ack;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "GooglePubSubSubscriberService - MessageId: {messageId}",
                    message.MessageId);

                return SubscriberClient.Reply.Nack;
            }
        });

        if (task != null) await task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<ServiceResponse> SendMessageAsync(int processCount)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var messageService = scope.ServiceProvider.GetService<IMessageService>();

        var message = await messageService!.SendAsync(new MessageRequest(
            _configuration.GetValue<string>("MESSAGE_SERVICE_FROM")!,
            _configuration.GetValue<string>("MESSAGE_SERVICE_TO")!,
            new List<MessageContentRequest>
            {
                new($"Atenção! Você tem um total de {processCount} processo(s) não analisado(s). Acesse https://bit.ly/3gtEHEB para mais informações.")
            }));

        return message;
    }

    public void Dispose()
    {
        _subscriberClient.StopAsync(new CancellationTokenSource().Token);

        _logger.LogInformation(
            "GooglePubSubSubscriberService - Disposing - ProjectId: {projectId} - Subscription: {subscription}",
            _configuration["PROJECT_ID"], _configuration["LEGAL_PROCESS_SUBSCRIPTION_ID"]);
    }
}
