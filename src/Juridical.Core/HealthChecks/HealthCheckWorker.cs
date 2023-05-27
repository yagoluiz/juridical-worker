using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Juridical.Core.HealthChecks;

public class HealthCheckWorker : BackgroundService
{
    private readonly HttpListener _httpListener;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HealthCheckWorker> _logger;

    public HealthCheckWorker(
        IConfiguration configuration,
        ILogger<HealthCheckWorker> logger)
    {
        _httpListener = new HttpListener();
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var baseUri = _configuration.GetValue<string>("HEALTH_CHECK_URI");

        _httpListener.Prefixes.Add($"{baseUri}/health/liveness/");
        _httpListener.Prefixes.Add($"{baseUri}/health/readiness/");
        _httpListener.Start();

        _logger.LogInformation("HealthCheckWorker - Listening health check...");

        while (!stoppingToken.IsCancellationRequested)
        {
            HttpListenerContext? context = null;

            try
            {
                context = await _httpListener.GetContextAsync();
            }
            catch (HttpListenerException exception)
            {
                if (exception.ErrorCode == 995)
                {
                    _logger.LogCritical(exception, "HealthCheckWorker - Message error: {message}", exception.Message);
                    return;
                }
            }

            if (context is null) continue;

            await WriteResponseAsync(context, stoppingToken);
        }
    }

    private static async Task WriteResponseAsync(HttpListenerContext? context, CancellationToken stoppingToken)
    {
        var response = context!.Response;
        response.ContentType = "text/plain";
        response.Headers.Add(HttpResponseHeader.CacheControl, "no-store, no-cache");
        response.StatusCode = (int)HttpStatusCode.OK;

        var messageBytes = "Healthy"u8.ToArray();
        response.ContentLength64 = messageBytes.Length;

        await response.OutputStream.WriteAsync(messageBytes, 0, messageBytes.Length, stoppingToken);

        response.OutputStream.Close();
        response.Close();
    }
}
