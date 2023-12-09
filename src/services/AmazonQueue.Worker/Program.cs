using AmazonQueue.MessageBus.Configuration;
using AmazonQueue.Worker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;
        services.AddMessageBus(configuration);

        services.AddHostedService<WorkerCart>();
        services.AddHostedService<WorkerUserAuth>();
        services.AddHostedService<WorkerUserCustomer>();
    })
    .Build();

await host.RunAsync();
