namespace TelegramService.TelegramAccess;

public class TelegramUpdate
{
    public Message Message { get; set; }
}

public class Message
{
    public long MessageId { get; set; }
    public From From { get; set; }
    public Chat Chat { get; set; }
    public string Text { get; set; }
}

public class From
{
    public long Id { get; set; }
    public bool IsBot { get; set; }
    public string FirstName { get; set; }
    public string Username { get; set; }
    public string LanguageCode { get; set; }
}

public class Chat
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string Username { get; set; }
    public string Type { get; set; }
}
