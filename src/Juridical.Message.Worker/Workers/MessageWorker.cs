using Juridical.Core.Interfaces;
using Juridical.Core.Models.Requests;
using Juridical.Core.Models.Responses;

namespace Juridical.Message.Worker.Workers;

public class MessageWorker : BackgroundService
{
    private readonly IMessageService _messageService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MessageWorker> _logger;

    public MessageWorker(
        IMessageService messageService,
        IConfiguration configuration,
        ILogger<MessageWorker> logger)
    {
        _messageService = messageService;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
    
    private async Task SendMessageAsync(int processCount, CancellationToken stoppingToken)
    {
        var message = await _messageService.SendAsync(new MessageRequest(
            _configuration.GetValue<string>("MESSAGE_SERVICE_FROM"),
            _configuration.GetValue<string>("MESSAGE_SERVICE_TO"),
            new List<MessageContentRequest>
            {
                new($"Atenção! Você tem um total de {processCount} processo(s) não analisado(s). Acesse https://bit.ly/3gtEHEB para mais informações.")
            }));

        if (!message.Success)
        {
            _logger.LogCritical($"LegalProcessWorker send error message: {message.Content}");
        }
        else
        {
            _logger.LogInformation($"LegalProcessWorker send success message: {(message.Content as MessageResponse)?.Id}");

            //TODO: Refactor memory cache
            // _memoryCache.Set(CacheKey, processCount, new MemoryCacheEntryOptions()
            //     .AddExpirationToken(new CancellationChangeToken(stoppingToken)));
        }
    }
}
