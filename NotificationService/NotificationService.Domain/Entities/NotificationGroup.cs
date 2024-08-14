namespace NotificationService.Domain.Entities;

public class NotificationGroup
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public ICollection<UserGroup> UserGroups { get; set; }
}