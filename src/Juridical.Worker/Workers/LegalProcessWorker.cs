using Juridical.Worker.Builders;
using Juridical.Worker.Interfaces;
using Juridical.Worker.Models.Requests;
using Juridical.Worker.Models.Responses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Juridical.Worker.Workers;

public class LegalProcessWorker : BackgroundService
{
    private readonly IMessageService _messageService;
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LegalProcessWorker> _logger;

    private const string CacheKey = "LegalProcessKey";

    public LegalProcessWorker(
        IMessageService messageService,
        IMemoryCache memoryCache,
        IConfiguration configuration,
        ILogger<LegalProcessWorker> logger)
    {
        _messageService = messageService;
        _memoryCache = memoryCache;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("LegalProcessWorker running at: {time}", DateTimeOffset.Now);

            try
            {
                var initHourActive = DateTime.Now.Hour >= _configuration.GetValue<int>("INIT_ACTIVE_IN_HOURS");
                
                _logger.LogInformation($"LegalProcessWorker init hour active: {initHourActive}");
                
                if (initHourActive)
                {
                    var legalProcessBuilder = new LegalProcessBuilder(_configuration.GetValue<string>("WEB_DRIVER_URI"))
                        .LoginPage(
                            _configuration.GetValue<string>("LEGAL_PROCESS_URL"),
                            _configuration.GetValue<string>("LEGAL_PROCESS_USER"),
                            _configuration.GetValue<string>("LEGAL_PROCESS_PASSWORD"))
                        .ProcessPage(_configuration.GetValue<string>("LEGAL_PROCESS_SERVICE_KEY"))
                        .ProcessCount()
                        .Quit()
                        .Build();

                    var processCount = legalProcessBuilder.ProcessCount;
                    var messageServiceActive = _configuration.GetValue<bool>("MESSAGE_SERVICE_ACTIVE");

                    _logger.LogInformation($"LegalProcessWorker process count: {processCount}");
                    _logger.LogInformation($"LegalProcessWorker MESSAGE_SERVICE_ACTIVE: {messageServiceActive}");

                    if (messageServiceActive) await ProcessCountAsync(processCount, stoppingToken);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"LegalProcessWorker exception message: {exception.Message}");
                _logger.LogError($"LegalProcessWorker exception stack trace: {exception.StackTrace}");
            }

            _logger.LogInformation("LegalProcessWorker running finish at: {time}", DateTimeOffset.Now);

            await Task.Delay(_configuration.GetValue<int>("LEGAL_PROCESS_EXECUTE_IN_MILLISECONDS"), stoppingToken);
        }
    }

    private async Task ProcessCountAsync(int processCount, CancellationToken stoppingToken)
    {
        var cachedProcessCount = await _memoryCache.GetOrCreateAsync(
            CacheKey,
            cacheEntry =>
            {
                cacheEntry.AddExpirationToken(new CancellationChangeToken(stoppingToken));
                return Task.FromResult(0);
            });

        _logger.LogInformation($"LegalProcessWorker cached process count: {cachedProcessCount}");
        _logger.LogInformation($"LegalProcessWorker process count: {processCount}");

        var sendMessage = cachedProcessCount != processCount;

        _logger.LogInformation($"LegalProcessWorker process count send message: {sendMessage}");

        if (sendMessage) await SendMessageAsync(processCount, stoppingToken);
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
            return;
        }

        _logger.LogInformation($"LegalProcessWorker send success message: {(message.Content as MessageResponse)?.Id}");

        _memoryCache.Set(CacheKey, processCount, new MemoryCacheEntryOptions()
            .AddExpirationToken(new CancellationChangeToken(stoppingToken)));
    }
}
