using Juridical.Message.Worker;
using Juridical.Message.Worker.Workers;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => { services.AddHostedService<MessageWorker>(); })
    .Build();

await host.RunAsync();