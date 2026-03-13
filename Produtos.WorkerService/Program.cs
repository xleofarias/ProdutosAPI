using WorkerService;
using WorkerService.Consumers;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProductCreatedConsumer>();

    var rabbitConnection = Environment.GetEnvironmentVariable("RabbitConnectionString") ?? builder.Configuration.GetConnectionString("Rabbit");
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitConnection);

        // 3. A MÁGICA: Essa linha manda o MassTransit criar a Fila (Queue) 
        // automaticamente lá no CloudAMQP e plugar no nosso Consumidor!
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
