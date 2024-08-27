using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TelegramService.Domain.Abstractions;
using TelegramService.MessageBrokerAccess.Models;

namespace TelegramService.MessageBrokerAccess;

public class RabbitMqListener : BackgroundService
{
    private readonly ILogger<RabbitMqListener> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqListener(
        ILogger<RabbitMqListener> logger,
        IServiceScopeFactory serviceScopeFactory
        )
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;

        // var factory = new ConnectionFactory() { HostName = "localhost" };
        var uri = Environment.GetEnvironmentVariable("ConnectionStrings__RabbitMQ");

        if (uri is null)
        {
            // TODO
            // _logger.LogCritical("lkasjd");
            uri = "amqp://guest:guest@localhost:5672";
        }
        
        // var factory = new ConnectionFactory() { Uri = new Uri(uri) };
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: "monitoring_out_queue",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }
    
    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            await ProcessMessage(message);
        };
        _channel.BasicConsume(
            queue: "monitoring_out_queue",
            autoAck: true,
            consumer: consumer);

        return Task.CompletedTask;
    }

    private async Task ProcessMessage(string messageString)
    {
        _logger.LogInformation("Received From MessageBroker {Message}", messageString);

        EventsMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<EventsMessage>(messageString);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while Deserialize messageString from broker");
            return;
        }
        
        if (message is null)
            return;

        using var scope = _serviceScopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var telegramMessageSender = scope.ServiceProvider.GetRequiredService<ITelegramMessageSender>();
        var brokerSender = scope.ServiceProvider.GetRequiredService<IBrokerSender>();
        
        foreach (var messageEvent in message.Events)
        {
            var userId = new Guid(messageEvent.UserId);
            var user = userRepository.GetUserById(userId);

            if (user is null)
            {
                _logger.LogError("No user");
                continue;
            }

            var notification = CreateNotificationMessage(messageEvent.Type, messageEvent.MessageParams);
            
            var isOk = await telegramMessageSender.SendMessageAsync(user.ChatId, notification);

            if (!isOk)
            {
                _logger.LogError("Not send");
                continue;
            }

            // TODO
            // await _brokerSender.SendMessage("Ok eventId");
        }
    }

    private string CreateNotificationMessage(string type, MessageParam[] messageParams)
    {
        StringBuilder sb = new();
        sb.Append('*');
        sb.Append(type);
        sb.Append('\n');
        foreach (var param in messageParams)
        {
            sb.Append(param.Name);
            sb.Append(' ');
            sb.Append(param.Value);
            sb.Append('\n');
        }

        return sb.ToString();
    }
}