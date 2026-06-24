using WorkerService.Consumers;
using MassTransit;
using DotNetEnv;

Env.Load("../.env");

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProductCreatedConsumer>();

    var rabbitConnection = Environment.GetEnvironmentVariable("RABBITMQ_URL") ?? builder.Configuration.GetConnectionString("Rabbit");
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitConnection);

        // 3. A M�GICA: Essa linha manda o MassTransit criar a Fila (Queue) 
        // automaticamente l� no CloudAMQP e plugar no nosso Consumidor!
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
