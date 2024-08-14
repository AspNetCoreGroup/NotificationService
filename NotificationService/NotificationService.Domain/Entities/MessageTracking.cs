namespace NotificationService.Domain.Entities;

public class MessageTracking
{
    public long MessageId { get; set; }
    public int MessageDeliveryStatusId { get; set; }
    public MessageDeliveryStatus MessageDeliveryStatus { get; set; }
    public int PriorityId { get; set; }
    public Priority Priority { get; set; }
    public int SubscriptionTypeId { get; set; }
    public SubscriptionType SubscriptionType { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public int UserId { get; set; }
    public User User { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public int RetryCount { get; set; }
}