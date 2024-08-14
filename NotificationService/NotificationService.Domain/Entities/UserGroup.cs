namespace NotificationService.Domain.Entities;

public class UserGroup
{
    public int UserId { get; set; }
    public User User { get; set; }
    public int GroupId { get; set; }
    public NotificationGroup NotificationGroup { get; set; }
}