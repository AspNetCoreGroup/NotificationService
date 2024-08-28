namespace NotificationService.Domain.Dtos;

public class AuthUser
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Telegram { get; set; }
    public string Email { get; set; }
    public long TelegramChatId { get; set; }
}