using TelegramService.Domain.Abstractions;
using TelegramService.Domain.Abstractions.Repositories;
using TelegramService.Domain.Entities;
using TelegramService.Domain.Models;
using TelegramService.TelegramAccess;

namespace TelegramService.Api.Services;

public interface IRegistrationService
{
    public Task<string> TryRegister(TelegramUpdate update);
}

public class RegistrationService : IRegistrationService
{
    private readonly ILogger<RegistrationService> _logger;
    private readonly IRegistrationCodeRepository _registrationCodeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBrokerSender _brokerSender;

    public RegistrationService(
        ILogger<RegistrationService> logger,
        IRegistrationCodeRepository registrationCodeRepository,
        IUserRepository userRepository,
        IBrokerSender brokerSender
        )
    {
        _logger = logger;
        _registrationCodeRepository = registrationCodeRepository;
        _userRepository = userRepository;
        _brokerSender = brokerSender;
    }
    
    public async Task<string> TryRegister(TelegramUpdate? update)
    {
        if (update is null)
        {
            _logger.LogError("Bad update telegram message");
        }
        
        var chatId = update.Message.Chat.Id;
        var messageText = update.Message.Text;
        
        _logger.LogInformation("Received a message from chat Id: {chatId}, Message: {messageText}", 
            chatId, messageText);

        var userIdFromCode = _registrationCodeRepository.GetUserIdFromCode(messageText);

        if (userIdFromCode is null)
        {
            // TODO не нравится хардкод сообщений
            return "Код не привязан к пользователю";
        }
        
        var user = _userRepository.GetUserById(userIdFromCode.Value);
        _logger.LogDebug("User {@User}", user);

        string messageToUser;
        if (user == default)
        {
            _userRepository.CreateUser(new User(){ ChatId = chatId, UserId = userIdFromCode.Value });
            await _brokerSender.SendRegistrationStatus(new UserTelegramChatRegistration() 
                { TelegramChatId = chatId, UserId = userIdFromCode.Value });
            messageToUser = "Чат успешно привязан к пользователю";
        }
        else
        {
            messageToUser = user.ChatId == chatId 
                ? "Чат уже привязан к вам" 
                : "Пользователь уже привязан к другому чату";
        }
    
        return messageToUser;
    }
}