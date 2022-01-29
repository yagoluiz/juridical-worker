using Juridical.Worker.Workers;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<LegalProcessWorker>();
    })
    .Build();

await host.RunAsync();
