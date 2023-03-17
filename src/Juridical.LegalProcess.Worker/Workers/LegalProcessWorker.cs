using Juridical.Core.Builders;
using Juridical.Core.Events;
using Juridical.Core.Interfaces;
using Juridical.Core.Models.Publishers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Juridical.LegalProcess.Worker.Workers;

public class LegalProcessWorker : BackgroundService
{
    private readonly IPublisherService _publisherService;
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LegalProcessWorker> _logger;

    private const string CacheKey = "LegalProcessKey";

    public LegalProcessWorker(
        IPublisherService publisherService,
        IMemoryCache memoryCache,
        IConfiguration configuration,
        ILogger<LegalProcessWorker> logger)
    {
        _publisherService = publisherService;
        _memoryCache = memoryCache;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("LegalProcessWorker - Start running: {time}", DateTimeOffset.Now);

            await ProcessAsync(stoppingToken);

            _logger.LogInformation("LegalProcessWorker - Finish running: {time}", DateTimeOffset.Now);

            await Task.Delay(_configuration.GetValue<int>("LEGAL_PROCESS_EXECUTE_IN_MILLISECONDS"), stoppingToken);
        }
    }

    private async Task ProcessAsync(CancellationToken stoppingToken)
    {
        try
        {
            var initHourActive = DateTime.Now.Hour >= _configuration.GetValue<int>("INIT_ACTIVE_IN_HOURS");

            _logger.LogInformation("LegalProcessWorker - INIT_ACTIVE_IN_HOURS: {initHourActive}", initHourActive);

            if (initHourActive)
            {
                var legalProcessBuilder =
                    new LegalProcessBuilder(_configuration.GetValue<string>("WEB_DRIVER_URI")!)
                        .LoginPage(
                            _configuration.GetValue<string>("LEGAL_PROCESS_URL")!,
                            _configuration.GetValue<string>("LEGAL_PROCESS_USER")!,
                            _configuration.GetValue<string>("LEGAL_PROCESS_PASSWORD")!)
                        .ProcessPage(_configuration.GetValue<string>("LEGAL_PROCESS_SERVICE_KEY")!)
                        .ProcessCount()
                        .LogoffPage(_configuration.GetValue<string>("LEGAL_PROCESS_URL")!)
                        .Quit()
                        .Build();

                var processCount = legalProcessBuilder.ProcessCount;
                var messageServiceActive = _configuration.GetValue<bool>("MESSAGE_SERVICE_ACTIVE");

                _logger.LogInformation("LegalProcessWorker - Process count: {processCount}", processCount);
                _logger.LogInformation("LegalProcessWorker - MESSAGE_SERVICE_ACTIVE: {messageServiceActive}",
                    messageServiceActive);

                if (messageServiceActive) await ProcessCountAsync(processCount, stoppingToken);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "LegalProcessWorker - Message error: {message}", exception.Message);
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

        _logger.LogInformation("LegalProcessWorker - Cached process count: {cachedProcessCount}", cachedProcessCount);
        _logger.LogInformation("LegalProcessWorker - Process count: {processCount}", processCount);

        var sendMessage = cachedProcessCount != processCount;

        _logger.LogInformation("LegalProcessWorker - Send process count: {sendMessage}", sendMessage);

        if (sendMessage)
        {
            await _publisherService.PublishAsync(Message.FromEvent(new LegalProcessEvent(processCount)));

            _memoryCache.Set(CacheKey, processCount, new MemoryCacheEntryOptions()
                .AddExpirationToken(new CancellationChangeToken(stoppingToken)));
        }
    }
}
