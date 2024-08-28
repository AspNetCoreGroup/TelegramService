using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using TelegramService.Domain.Abstractions;
using TelegramService.Domain.Models;

namespace TelegramService.MessageBrokerAccess;

public class RabbitMqSender : IBrokerSender, IDisposable
{
    private readonly ILogger<RabbitMqSender> _logger;
    
    private readonly IConnection _connection;
    private readonly IModel _channel;
    
    private const string GetStatusQueue = "telegram_get_message_status_queue";
    private const string GetRegistrationQueue = "telegram_get_registration_queue";

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
        
        // _logger.LogInformation("Broker uri: {Uri}", uri);

        if (uri is null)
        {
            _logger.LogCritical("No uri for telegram rabbitmq");
            throw new Exception("No uri for telegram rabbitmq");
        }
        
        var factory = new ConnectionFactory() { Uri = new Uri(uri) };
        // var factory = new ConnectionFactory() { HostName = "localhost" };
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.QueueDeclare(queue: GetStatusQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: GetRegistrationQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
    }
    
    public void Dispose()
    {
        _channel.Close();
        _connection.Close();
    }
    
    public Task SendMessageStatus(MessageStatus messageStatus)
    {
        var message = JsonSerializer.Serialize(messageStatus);
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(exchange: "",
            routingKey: GetStatusQueue,
            basicProperties: null,
            body: body);

        return Task.CompletedTask;
    }

    public Task SendRegistrationStatus(UserTelegramChatRegistration registration)
    {
        var message = JsonSerializer.Serialize(registration);
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(exchange: "",
            routingKey: GetRegistrationQueue,
            basicProperties: null,
            body: body);

        return Task.CompletedTask;
    }
}