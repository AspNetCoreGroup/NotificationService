namespace NotificationService.MessageBrokerAccess.Entities;

public class MessageRecievedEventArgs
{
    public required string QueueName { get; set; }
    public required string Message { get; set; }
    public required Dictionary<string, string> Param { get; set; }
    public bool Hadled { get; set; }
    public bool Failed { get; set; }
    public bool Rejected { get; set; }
    public bool Resended { get; set; }
}