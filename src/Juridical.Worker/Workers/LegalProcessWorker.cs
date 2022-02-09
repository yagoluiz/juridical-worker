using Juridical.Worker.Interfaces;
using Juridical.Worker.Models.Requests;
using Juridical.Worker.Models.Responses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace Juridical.Worker.Workers;

public class LegalProcessWorker : BackgroundService
{
    private readonly IMessageService _messageService;
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LegalProcessWorker> _logger;

    private const string CacheKey = "ProcessKey";

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

            var webDriver = new RemoteWebDriver(
                new Uri(_configuration.GetValue<string>("WEB_DRIVER_URI")), new ChromeOptions());

            try
            {
                webDriver.Navigate().GoToUrl(_configuration.GetValue<string>("LEGAL_PROCESS_URL"));

                webDriver.FindElementById("login").SendKeys(_configuration.GetValue<string>("LEGAL_PROCESS_USER"));
                webDriver.FindElementById("senha").SendKeys(_configuration.GetValue<string>("LEGAL_PROCESS_PASSWORD"));
                webDriver.FindElementByName("entrar").Click();

                webDriver
                    .FindElementByXPath(
                        $"//*[contains(text(),'{_configuration.GetValue<string>("LEGAL_PROCESS_SERVICE_KEY")}')]")
                    .Click();

                webDriver.SwitchTo().Frame(webDriver.FindElement(By.Name("userMainFrame")));

                var processCount = 0;

                var table = webDriver.FindElementByTagName("table");
                var tableBody = table.FindElement(By.TagName("tbody"));

                var tableRows = tableBody.FindElements(By.TagName("tr"));

                foreach (var tableRow in tableRows)
                {
                    if (processCount > 0) break;

                    var contentRows = tableRow.FindElements(By.TagName("td"));

                    foreach (var contentRow in contentRows)
                    {
                        var attributeRow = contentRow.GetAttribute("class");

                        if (attributeRow != "colunaMinima") continue;

                        var content = tableRow.FindElement(By.TagName("a")).Text;

                        if (content is null) continue;

                        processCount = int.Parse(content);
                        break;
                    }
                }

                _logger.LogInformation($"LegalProcessWorker process count: {processCount}");

                if (_configuration.GetValue<bool>("MESSAGE_SERVICE_ACTIVE") && processCount > 0)
                {
                    _logger.LogInformation("LegalProcessWorker MESSAGE_SERVICE_ACTIVE activated");

                    var cachedProcessCount = await _memoryCache.GetOrCreateAsync(
                        CacheKey, 
                        cacheEntry =>
                        {
                            cacheEntry.AddExpirationToken(new CancellationChangeToken(stoppingToken));
                            return Task.FromResult(processCount);
                        });

                    _logger.LogInformation($"LegalProcessWorker cached process count: {cachedProcessCount}");

                    if (cachedProcessCount == 1 || cachedProcessCount != processCount)
                    {
                        var message = await _messageService.SendAsync(new MessageRequest(
                            _configuration.GetValue<string>("MESSAGE_SERVICE_FROM"), 
                            _configuration.GetValue<string>("MESSAGE_SERVICE_TO"), 
                            new List<MessageContentRequest>
                            {
                                new($"Atenção! Você tem um total de {processCount} processo(s) não analisado(s). Acesse https://bit.ly/3gtEHEB para mais informações.")
                            }));

                        if (message.Success)
                        {
                            _logger.LogInformation($"LegalProcessWorker send success message: {(message.Content as MessageResponse)?.Id}");

                            _memoryCache.Set(CacheKey, processCount, new MemoryCacheEntryOptions()
                                .AddExpirationToken(new CancellationChangeToken(stoppingToken)));
                        }
                        else
                        {
                            _logger.LogCritical($"LegalProcessWorker send error message: {message.Content}");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"LegalProcessWorker exception message: {exception.Message}");
                _logger.LogError($"LegalProcessWorker exception stack trace: {exception.StackTrace}");
            }
            finally
            {
                webDriver.Quit();
            }

            _logger.LogInformation("LegalProcessWorker running finish at: {time}", DateTimeOffset.Now);

            await Task.Delay(_configuration.GetValue<int>("LEGAL_PROCESS_EXECUTE_IN_MILLISECONDS"), stoppingToken);
        }
    }
}
