using WorkerService.Consumers;
using MassTransit;
using DotNetEnv;

Env.Load("../.env");

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProductCreatedConsumer>();

    x.UsingAzureServiceBus((context, cfg) =>
    {
        cfg.Host(Environment.GetEnvironmentVariable("AZURE_BUS_URL"));

        cfg.ReceiveEndpoint("product-created", endpoint =>
        {
            endpoint.ConfigureConsumeTopology = false;

            endpoint.ConfigureConsumer<ProductCreatedConsumer>(context);
        });
    });
});


var host = builder.Build();
host.Run();
