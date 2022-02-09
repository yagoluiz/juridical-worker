using Juridical.Worker.Interfaces;
using Juridical.Worker.Services;
using Juridical.Worker.Workers;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<LegalProcessWorker>();
        services.AddHttpClient<IMessageService, MessageService>(client =>
        {
            client.BaseAddress = new Uri(context.Configuration.GetValue<string>("MESSAGE_SERVICE_URL"));
            client.DefaultRequestHeaders.Add("x-api-token",
                context.Configuration.GetValue<string>("MESSAGE_SERVICE_API_TOKEN"));
        });
        services.AddMemoryCache();
    })
    .Build();

await host.RunAsync();
