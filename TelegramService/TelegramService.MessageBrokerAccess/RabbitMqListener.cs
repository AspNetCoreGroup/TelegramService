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
    // private readonly ITelegramMessageSender _telegramMessageSender;
    // private readonly IUserRepository _userRepository;
    // private readonly IBrokerSender _brokerSender;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqListener(
        ILogger<RabbitMqListener> logger,
        // ITelegramMessageSender telegramMessageSender,
        // IUserRepository userRepository,
        // IBrokerSender brokerSender,
        IServiceScopeFactory serviceScopeFactory
        )
    {
        _logger = logger;
        // _telegramMessageSender = telegramMessageSender;
        // _userRepository = userRepository;
        // _brokerSender = brokerSender;
        _serviceScopeFactory = serviceScopeFactory;

        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: "test_queue",
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
            queue: "test_queue",
            autoAck: true,
            consumer: consumer);

        return Task.CompletedTask;
    }

    private async Task ProcessMessage(string messageString)
    {
        _logger.LogInformation("Received From MessageBroker {Message}", messageString);
        
        

        var message = JsonSerializer.Deserialize<EventsMessage>(messageString);

        using var scope = _serviceScopeFactory.CreateScope();
        var _userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var _telegramMessageSender = scope.ServiceProvider.GetRequiredService<ITelegramMessageSender>();
        var _brokerSender = scope.ServiceProvider.GetRequiredService<IBrokerSender>();
        
        foreach (var messageEvent in message.Events)
        {
            var userId = new Guid(messageEvent.UserId);
            var user = _userRepository.GetUserById(userId);

            if (user is null)
            {
                _logger.LogError("No user");
                continue;
            }

            var notification = CreateNotificationMessage(messageEvent.Type, messageEvent.MessageParams);
            
            var isOk = await _telegramMessageSender.SendMessageAsync(user.ChatId, notification);

            if (!isOk)
            {
                _logger.LogError("Not send");
                continue;
            }

            await _brokerSender.SendMessage("Ok eventId");
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