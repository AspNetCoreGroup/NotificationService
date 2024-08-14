namespace NotificationService.Domain.Entities;

public class MessageDeliveryStatus
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public ICollection<MessageTracking> MessageTrackings { get; set; } 
}