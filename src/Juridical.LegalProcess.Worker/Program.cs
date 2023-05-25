using Juridical.Core.HealthChecks;
using Juridical.Core.Interfaces;
using Juridical.Core.Services;
using Juridical.LegalProcess.Worker.Workers;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton<IPublisherService, GooglePubSubPublisherService>();
        services.AddMemoryCache();
        services.AddHostedService<LegalProcessWorker>();
        services.AddHostedService<HealthCheckWorker>();
    })
    .Build();

await host.RunAsync();
