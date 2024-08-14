namespace NotificationService.Domain.Entities;

public class SubscriptionType
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public ICollection<UserSubscription> UserSubscriptions { get; set; }
    public ICollection<MessageTracking> MessageTrackings { get; set; }
}