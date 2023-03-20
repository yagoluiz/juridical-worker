using Juridical.Core.Interfaces;

namespace Juridical.Message.Worker.Workers;

public class MessageWorker : BackgroundService
{
    private readonly ISubscriberService _subscriberService;
    private readonly ILogger<MessageWorker> _logger;

    public MessageWorker(
        ISubscriberService subscriberService,
        ILogger<MessageWorker> logger)
    {
        _subscriberService = subscriberService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("MessageWorker - Start running: {time}", DateTimeOffset.Now);

            await _subscriberService.SubscriberAsync(stoppingToken);
        }

        _subscriberService.Dispose();
    }
}
