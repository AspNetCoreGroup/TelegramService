using Microsoft.AspNetCore.Mvc;
using TelegramService.DataAccess;
using TelegramService.Domain.Abstractions;
using TelegramService.Domain.Abstractions.Repositories;
using TelegramService.Domain.Entities;
using TelegramService.TelegramAccess;

namespace TelegramService.Api.Controllers;

[ApiController]
[Route("/api/user")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IRegistrationCodeRepository _registrationCodeRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITelegramMessageSender _telegramMessageSender;

    public UserController(
        ILogger<UserController> logger,
        IRegistrationCodeRepository registrationCodeRepository,
        IUserRepository userRepository,
        ITelegramMessageSender telegramMessageSender)
    {
        _logger = logger;
        _registrationCodeRepository = registrationCodeRepository;
        _userRepository = userRepository;
        _telegramMessageSender = telegramMessageSender;
    }
    
    [HttpPost("{userId:guid}/code/{code}")]
    public async Task<ActionResult> AddRegisterCodeToUser(
        Guid userId, string code)
    {
        var registrationCode = new RegistrationCode(){ UserId = userId, Code = code, CreationDateTime = DateTime.Now};
        _registrationCodeRepository.AddCode(registrationCode);
        
        return Ok();
    }

    [HttpPost("/update")]
    public async Task<ActionResult> MessageFromBot([FromBody] TelegramUpdate update)
    {
        var chatId = update.Message.Chat.Id;
        var messageText = update.Message.Text;
            
        // TODO потом удалить логирование
        _logger.LogInformation("Received a message from chat Id: {chatId}, Message: {messageText}", 
            chatId, messageText);

        var userIdFromCode = _registrationCodeRepository.GetUserIdFromCode(messageText);

        if (userIdFromCode is null)
        {
            await _telegramMessageSender.SendMessageAsync(chatId, "Код не привязан к пользователю");
            return Ok();
        }
        
        var user = _userRepository.GetUserById(userIdFromCode.Value);
        _logger.LogInformation("User {@User}", user);

        string messageToUser;
        if (user == default)
        {
            _userRepository.CreateUser(new User(){ ChatId = chatId, UserId = Guid.NewGuid() });
            // TODO не нравится хардкод сообщения
            messageToUser = "Чат успешно привязан к пользователю";
        }
        else
        {
            if (user.ChatId == chatId)
            {
                messageToUser = "Чат уже привязан к вам";
            }
            else
            {
                messageToUser = "Пользователь уже привязан к другому чату";
            }
        }
        
        await _telegramMessageSender.SendMessageAsync(chatId, messageToUser);

        return Ok();
    }
}