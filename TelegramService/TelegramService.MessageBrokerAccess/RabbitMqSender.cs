using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using TelegramService.Domain.Abstractions;

namespace TelegramService.MessageBrokerAccess;

public class RabbitMqSender : IBrokerSender
{
    private readonly ILogger<RabbitMqSender> _logger;

    public RabbitMqSender(ILogger<RabbitMqSender> logger)
    {
        _logger = logger;
    }
    
    public Task SendMessage(string message)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "test_queue",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "",
            routingKey: "test_queue",
            basicProperties: null,
            body: body);

        return Task.CompletedTask;
    }
}