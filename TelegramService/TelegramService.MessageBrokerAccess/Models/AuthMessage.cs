namespace TelegramService.MessageBrokerAccess.Models;

public class AuthMessage
{
    public string Operation { get; set; }
    public AuthUser User { get; set; } 
}

public class AuthUser
{
    public string UserId { get; set; }
    public string UserFullName { get; set; }
    public string TelegramId { get; set; }
    public string Email { get; set; }
}