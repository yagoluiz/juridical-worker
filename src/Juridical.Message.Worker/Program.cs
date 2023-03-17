using Juridical.Core.Interfaces;
using Juridical.Core.Services;
using Juridical.Message.Worker.Subscribers;
using Juridical.Message.Worker.Workers;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<ISubscriberService, GooglePubSubSubscriberService>();
        services.AddHttpClient<IMessageService, ZenviaMessageService>(client =>
        {
            client.BaseAddress = new Uri(context.Configuration.GetValue<string>("MESSAGE_SERVICE_URL")!);
            client.DefaultRequestHeaders.Add("x-api-token",
                context.Configuration.GetValue<string>("MESSAGE_SERVICE_API_TOKEN"));
        });
        services.AddHostedService<MessageWorker>();
    })
    .Build();

await host.RunAsync();
