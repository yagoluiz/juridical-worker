using Juridical.Core.Builders;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Juridical.LegalProcess.Worker.Workers;

public class LegalProcessWorker : BackgroundService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LegalProcessWorker> _logger;

    private const string CacheKey = "LegalProcessKey";

    public LegalProcessWorker(
        IMemoryCache memoryCache,
        IConfiguration configuration,
        ILogger<LegalProcessWorker> logger)
    {
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

                _logger.LogInformation($"LegalProcessWorker INIT_ACTIVE_IN_HOURS: {initHourActive}");

                if (initHourActive)
                {
                    var legalProcessBuilder = new LegalProcessBuilder(_configuration.GetValue<string>("WEB_DRIVER_URI"))
                        .LoginPage(
                            _configuration.GetValue<string>("LEGAL_PROCESS_URL"),
                            _configuration.GetValue<string>("LEGAL_PROCESS_USER"),
                            _configuration.GetValue<string>("LEGAL_PROCESS_PASSWORD"))
                        .ProcessPage(_configuration.GetValue<string>("LEGAL_PROCESS_SERVICE_KEY"))
                        .ProcessCount()
                        .LogoffPage(_configuration.GetValue<string>("LEGAL_PROCESS_URL"))
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

        //TODO: Send topic message
        //if (sendMessage) await SendMessageAsync(processCount, stoppingToken);
    }
}
