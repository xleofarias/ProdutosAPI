using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkerService.Events;

namespace WorkerService.Consumers
{
    // A interface IConsumer<T> avisa o MassTransit: "Eu quero ouvir esse evento específico!"
    public class ProductCreatedConsumer : IConsumer<ProductCreatedEvent>
    {
        private readonly ILogger<ProductCreatedConsumer> _logger;

        public ProductCreatedConsumer(ILogger<ProductCreatedConsumer> logger)
        {
            _logger = logger;
        }
                                                 
        public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
        {
            var produto = context.Message; // Pega o JSON que chegou da fila

            _logger.LogInformation("Nova mensagem recebida do RabbitMQ!");
            _logger.LogInformation("Iniciando processamento do Produto: {Nome} (ID: {Id})", produto.Name, produto.ProductId);
            Console.WriteLine("Olha to aqui");
            // Simula um trabalho pesado e demorado de 3 segundos
            // A API lá no Render nem faz ideia que isso está acontecendo agora
            await Task.Delay(3000);

            _logger.LogInformation("Processamento concluído e salvo com sucesso!\n");
        }
    }
}
