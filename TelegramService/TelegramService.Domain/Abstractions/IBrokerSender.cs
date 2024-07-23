namespace TelegramService.Domain.Abstractions;

public interface IBrokerSender
{
    Task SendMessage(string message);
}