using Juridical.Core.Models.Builders;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace Juridical.Core.Builders;

public class LegalProcessBuilder
{
    private readonly RemoteWebDriver _webDriver;
    private readonly LegalProcess _legalProcess;

    public LegalProcessBuilder(string uri)
    {
        var options = new ChromeOptions();
        options.AddArguments("--no-sandbox", "--headless", "--disable-dev-shm-usage");

        _webDriver = new RemoteWebDriver(new Uri(uri), options);
        _legalProcess = new LegalProcess();
    }

    public LegalProcess Build() => _legalProcess;

    public LegalProcessBuilder LoginPage(string url, string user, string password)
    {
        _webDriver.Navigate().GoToUrl(url);
        _webDriver.FindElementById("login").SendKeys(user);
        _webDriver.FindElementById("senha").SendKeys(password);
        _webDriver.FindElementByName("entrar").Click();

        return this;
    }

    public LegalProcessBuilder ProcessPage(string serviceKey)
    {
        _webDriver.FindElementsByXPath($"//*[contains(text(),'{serviceKey}')]").Last().Click();
        _webDriver.SwitchTo().Frame(_webDriver.FindElement(By.Name("userMainFrame")));

        return this;
    }

    public LegalProcessBuilder ProcessCount()
    {
        var processCount = 0;

        var table = _webDriver.FindElementByTagName("table");
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

        _legalProcess.ProcessCount = processCount;

        return this;
    }

    public LegalProcessBuilder LogoffPage(string url)
    {
        _webDriver.Navigate().GoToUrl($"{url}/LogOn?PaginaAtual=-200");

        return this;
    }

    public LegalProcessBuilder Quit()
    {
        _webDriver.Quit();

        return this;
    }
}
