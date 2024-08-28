using TelegramService.Domain.Models;

namespace TelegramService.Domain.Abstractions;

public interface IBrokerSender
{
    Task SendMessageStatus(MessageStatus messageStatus);
    Task SendRegistrationStatus(UserTelegramChatRegistration registration);
}