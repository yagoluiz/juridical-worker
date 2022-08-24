using Juridical.LegalProcess.Worker.Workers;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddHostedService<LegalProcessWorker>();
        services.AddMemoryCache();
    })
    .Build();

await host.RunAsync();
