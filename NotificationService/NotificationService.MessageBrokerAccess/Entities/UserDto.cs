namespace NotificationService.MessageBrokerAccess.Entities;

public class UserDto
{
    public int UserID { get; set; }
    public string UserLogin { get; set; }
    public string NotificationEmail { get; set; }
    public string NotificationTelegramID { get; set; }
}