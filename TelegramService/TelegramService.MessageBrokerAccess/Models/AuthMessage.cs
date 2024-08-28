namespace TelegramService.MessageBrokerAccess.Models;

public class AuthMessage
{
    public string Operation { get; set; }
    public AuthUser User { get; set; } 
}

public class AuthUser
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Telegram { get; set; }
    public string Email { get; set; }
    public long TelegramChatId { get; set; }
}