using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using TelegramService.Domain.Abstractions;

namespace TelegramService.MessageBrokerAccess;

public class RabbitMqSender : IBrokerSender, IDisposable
{
    private readonly ILogger<RabbitMqSender> _logger;
    
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqSender(ILogger<RabbitMqSender> logger)
    {
        _logger = logger;
        
        // var rabbitMq = new RabbitMqConfiguration()
        // {
        //     HostName = Environment.GetEnvironmentVariable("RabbitMQ__HostName"),
        //     UserName = Environment.GetEnvironmentVariable("RabbitMQ__UserName"),
        //     Password = Environment.GetEnvironmentVariable("RabbitMQ__Password"),
        // };
        
        // var factory = new ConnectionFactory()
        // {
        //     HostName = rabbitMq.HostName,
        //     UserName = rabbitMq.UserName,
        //     Password = rabbitMq.Password
        // };
        
        var uri = Environment.GetEnvironmentVariable("ConnectionStrings__RabbitMQ");

        if (uri is null)
        {
            // TODO
            // _logger.LogCritical("lkasjd");
            uri = "amqp://guest:guest@rabbitmq:5672";
        }
        
        // var factory = new ConnectionFactory() { Uri = new Uri(uri) };
        var factory = new ConnectionFactory() { HostName = "localhost" };
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // queue: "monitoring_in_queue",
        _channel.QueueDeclare(
            queue: "monitoring_out_queue",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }
    
    public void Dispose()
    {
        _channel.Close();
        _connection.Close();
    }
    
    public Task SendMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(exchange: "",
            routingKey: "monitoring_out_queue",
            basicProperties: null,
            body: body);

        return Task.CompletedTask;
    }
}