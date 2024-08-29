namespace NotificationService.MessageBrokerAccess.Entities;

public class DataEventMessage<T> where T : class
{
    public DataEventOperationType Operation { get; set; }
    public T? Data { get; set; }
}

public enum DataEventOperationType : byte
{
    Add = 0,
    Update = 1,
    Delete = 2,
    Refresh = 3
}