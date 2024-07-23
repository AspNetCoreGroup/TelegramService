using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TelegramService.Api.Contacts.Requests;
using TelegramService.Domain.Abstractions;
using TelegramService.MessageBrokerAccess.Models;

namespace TelegramService.Api.Controllers;

[ApiController]
[Route("/api/notification")]
public class NotificationController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserRepository _userRepository;
    private readonly ITelegramMessageSender _telegramMessageSender;
    private readonly IBrokerSender _brokerSender;

    public NotificationController(
        ILogger<UserController> logger,
        IUserRepository userRepository,
        ITelegramMessageSender telegramMessageSender,
        IBrokerSender brokerSender
        )
    {
        _logger = logger;
        _userRepository = userRepository;
        _telegramMessageSender = telegramMessageSender;
        _brokerSender = brokerSender;
    }
    
    [HttpPost("user/{userId:guid}")]
    public async Task<ActionResult> NotifyUser(
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

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult> SendMessage(
        Guid userId)
    {
        var events = new EventsMessage
        {
            Events = new []
            {
                new Event() { UserId = "c78579b0-302e-42d2-a4c9-42f6f0165262", Type = "Error", MessageParams = new []
                {
                    new MessageParam() { Name = "Pressure", Value = "1"},
                    new MessageParam() { Name = "Temperature", Value = "36.6"}
                }}
            }
        };
        var request = JsonSerializer.Serialize(events);
        await _brokerSender.SendMessage(request);
        return Ok();
    }
}