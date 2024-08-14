namespace NotificationService.Domain.Entities;

public class UserSubscription
{
    public int UserId { get; set; }
    public User User { get; set; }
    public int SubscriptionTypeId { get; set; }
    public SubscriptionType SubscriptionType { get; set; }
}