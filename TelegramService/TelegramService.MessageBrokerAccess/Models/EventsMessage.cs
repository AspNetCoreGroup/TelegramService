namespace TelegramService.MessageBrokerAccess.Models;

public class EventsMessage
{
    public Event[] Events { get; set; }
}

public class Event
{
    public string UserId { get; set; }
    public string Type { get; set; }
    public MessageParam[] MessageParams { get; set; }
}

public class MessageParam
{
    public string Name { get; set; }
    public string Value { get; set; }
}