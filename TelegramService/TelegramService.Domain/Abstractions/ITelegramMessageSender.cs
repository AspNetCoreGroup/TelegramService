namespace TelegramService.Domain.Abstractions;

public interface ITelegramMessageSender
{
    Task<bool> SendMessageAsync(long chatId, string message);
}