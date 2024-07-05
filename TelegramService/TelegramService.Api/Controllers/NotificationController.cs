using Microsoft.AspNetCore.Mvc;
using TelegramService.Api.Contacts.Requests;
using TelegramService.Domain.Abstractions;

namespace TelegramService.Api.Controllers;

[ApiController]
[Route("/api/notification")]
public class NotificationController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserRepository _userRepository;
    private readonly ITelegramMessageSender _telegramMessageSender;

    public NotificationController(
        ILogger<UserController> logger,
        IUserRepository userRepository,
        ITelegramMessageSender telegramMessageSender
        )
    {
        _logger = logger;
        _userRepository = userRepository;
        _telegramMessageSender = telegramMessageSender;
    }
    
    
    
    [HttpPost("{userId:guid}/code/{code}")]
    public async Task<ActionResult> AddRegisterCodeToUser(
        Guid userId,
        [FromBody]NotificationRequest request)
    {
        var userInfo = _userRepository.GetUserById(userId);
        if (userInfo is null)
            return new NoContentResult();

        var chatId = userInfo.ChatId;
        var message = $"#{request.Type}\n\n{request.Message}";

        var isSuccess = await _telegramMessageSender.SendMessageAsync(chatId, message);
        if (!isSuccess)
        {
            _logger.LogError("Not send message to user {UserId}", userId);
            return BadRequest();
        }
        
        return Ok();
    }
}