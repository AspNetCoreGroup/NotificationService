namespace NotificationService.MessageBrokerAccess.Entities;

public class EventsMessage
{
    public Event[] Events { get; set; } = Enumerable.Empty<Event>().ToArray();
}

public class Event
{
    public int UserId { get; set; }
    public NotificationType Type { get; set; }
    public long MessageId { get; set; }
    public MessageParam[] MessageParams { get; set; } = Enumerable.Empty<MessageParam>().ToArray();
}

public class MessageParam
{
    public string Name { get; set; }
    public string Value { get; set; }
}

public class NotificationMessage
{
    public int UserID { get; set; }
    public NotificationType NotificationsType { get; set; }
    public List<MessageParam>? Params { get; set; }
}

public enum NotificationType : int
{
    Test = 0
}