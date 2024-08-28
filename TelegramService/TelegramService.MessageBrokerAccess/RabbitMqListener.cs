using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TelegramService.Domain.Abstractions;
using TelegramService.Domain.Abstractions.Repositories;
using TelegramService.Domain.Entities;
using TelegramService.Domain.Models;
using TelegramService.MessageBrokerAccess.Models;

namespace TelegramService.MessageBrokerAccess;

public class RabbitMqListener : BackgroundService
{
    private readonly ILogger<RabbitMqListener> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly IConnection _connection;
    private readonly IModel _channel;
    
    private const string SendEventQueue = "telegram_send_message_queue";
    private const string SendRegistrationQueue = "telegram_send_registration_queue";

    public RabbitMqListener(
        ILogger<RabbitMqListener> logger,
        IServiceScopeFactory serviceScopeFactory
        )
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        
        var uri = Environment.GetEnvironmentVariable("ConnectionStrings__RabbitMQ");

        if (uri is null)
        {
            _logger.LogCritical("No uri for telegram rabbitmq");
            throw new Exception("No uri for telegram rabbitmq");
            // uri = "amqp://guest:guest@localhost:5674/";
        }
        
        var factory = new ConnectionFactory() { Uri = new Uri(uri) };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: SendEventQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: SendRegistrationQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
    }
    
    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var eventsConsumer = new EventingBasicConsumer(_channel);
        eventsConsumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            await ProcessEventMessage(message);
        };
        _channel.BasicConsume(
            queue: SendEventQueue,
            autoAck: true,
            consumer: eventsConsumer);
        
        var registrationConsumer = new EventingBasicConsumer(_channel);
        registrationConsumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            await ProcessRegistrationMessage(message);
        };
        _channel.BasicConsume(
            queue: SendRegistrationQueue,
            autoAck: true,
            consumer: registrationConsumer);

        return Task.CompletedTask;
    }

    private async Task ProcessEventMessage(string messageString)
    {
        _logger.LogInformation("Received From MessageBroker {Message}", messageString);
        
        Event? message;
        try
        {
            message = JsonSerializer.Deserialize<Event>(messageString);
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
        
        var userId = message.UserId;
        var user = userRepository.GetUserById(userId);
        
        var messageStatus = new MessageStatus
        {
            MessageId = message.MessageId,
            IsSuccess = false
        };

        if (user is null)
        {
            _logger.LogError("No user in db with id {UserId}", userId);
            await brokerSender.SendMessageStatus(messageStatus);
            return;
        }

        try
        {
            var notification = CreateNotificationMessage("Test", message.MessageParams);

            _logger.LogInformation("Try to send message to user {UserId} with chatId {ChatId}", 
                user.UserId, user.ChatId);
            messageStatus.IsSuccess = await telegramMessageSender.SendMessageAsync(user.ChatId, notification);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while working with event message from broker");
        }
        finally
        {
            await brokerSender.SendMessageStatus(messageStatus);
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
    
    private async Task ProcessRegistrationMessage(string messageString)
    {
        _logger.LogInformation("Received From Registration queue: {Message}", messageString);
        
        AuthUser? authUser;
        try
        {
            authUser = JsonSerializer.Deserialize<AuthUser>(messageString);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while Deserialize messageString from broker");
            return;
        }
        
        if (authUser is null)
            return;
        
        using var scope = _serviceScopeFactory.CreateScope();
        var registrationCodeRepository = scope.ServiceProvider.GetRequiredService<IRegistrationCodeRepository>();

        try
        {
            registrationCodeRepository.AddCode(new RegistrationCode()
            {
                UserId = authUser.Id,
                Code = authUser.Telegram,
                CreationDateTime = DateTime.Now.ToUniversalTime()
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error to add code to user in db");
            return;
        }
        
        // var brokerSender = scope.ServiceProvider.GetRequiredService<IBrokerSender>();
        //
        // await brokerSender.SendRegistrationStatus(new UserTelegramChatRegistration()
        // {
        //     
        // });
    }
}