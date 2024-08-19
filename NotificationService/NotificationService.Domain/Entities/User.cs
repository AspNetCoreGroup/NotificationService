namespace NotificationService.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Telegram { get; set; }
    public long TelegramChatId { get; set; }
    
    public ICollection<MessageTracking> MessageTrackings { get; set; } 
    public ICollection<UserSubscription> UserSubscriptions { get; set; }
}