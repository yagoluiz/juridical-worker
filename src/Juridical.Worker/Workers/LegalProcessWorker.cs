using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace Juridical.Worker.Workers;

public class LegalProcessWorker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LegalProcessWorker> _logger;

    public LegalProcessWorker(IConfiguration configuration, ILogger<LegalProcessWorker> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("LegalProcessWorker running at: {time}", DateTimeOffset.Now);

            var webDriver = new RemoteWebDriver(
                new Uri(_configuration.GetValue<string>("WebDriver:Uri")),
                new ChromeOptions()
            );

            try
            {
                webDriver.Navigate().GoToUrl("https://www.google.com");
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

            await Task.Delay(1000, stoppingToken);
        }
    }
}
