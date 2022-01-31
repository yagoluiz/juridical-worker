using OpenQA.Selenium;
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
                webDriver.Navigate().GoToUrl(_configuration.GetValue<string>("LegalProcess:Url"));

                webDriver.FindElementById("login").SendKeys(_configuration.GetValue<string>("LegalProcess:User"));
                webDriver.FindElementById("senha").SendKeys(_configuration.GetValue<string>("LegalProcess:Password"));
                webDriver.FindElementByName("entrar").Click();

                webDriver
                    .FindElementByXPath(
                        $"//*[contains(text(),'{_configuration.GetValue<string>("LegalProcess:ServiceKey")}')]")
                    .Click();

                webDriver.SwitchTo().Frame(webDriver.FindElement(By.Name("userMainFrame")));

                var processes = 0;

                var table = webDriver.FindElementByTagName("table");
                var tableBody = table.FindElement(By.TagName("tbody"));

                var tableRows = tableBody.FindElements(By.TagName("tr"));

                foreach (var tableRow in tableRows)
                {
                    if (processes > 0) break;
                    
                    var contentRows = tableRow.FindElements(By.TagName("td"));

                    foreach (var contentRow in contentRows)
                    {
                        var attributeRow = contentRow.GetAttribute("class");

                        if (attributeRow != "colunaMinima") continue;

                        var content = tableRow.FindElement(By.TagName("a")).Text;

                        if (content is null) continue;

                        processes = int.Parse(content);
                        break;
                    }
                }

                _logger.LogInformation($"LegalProcessWorker processes count: {processes}");
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

            await Task.Delay(_configuration.GetValue<int>("LegalProcess:ExecuteInMilliseconds"), stoppingToken);
        }
    }
}
