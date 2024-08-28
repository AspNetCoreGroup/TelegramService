namespace TelegramService.MessageBrokerAccess.Models;

public class EventsMessage
{
    public Event[] Events { get; set; }
}

public class Event
{
    public int UserId { get; set; }
    public int Type { get; set; }
    public long MessageId { get; set; }
    public MessageParam[] MessageParams { get; set; }
}

public class MessageParam
{
    public string Name { get; set; }
    public string Value { get; set; }
}