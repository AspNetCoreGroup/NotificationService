using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Abstractions.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(int id);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task AddUserAsync(User? user);
    Task UpdateUserAsync(User? user);
    Task DeleteUserAsync(int id);
    Task AddSubscriptionToUserAsync(int userId, string subscriptionTypeCode);
    Task RemoveSubscriptionFromUserAsync(int userId, string subscriptionTypeCode);
    Task UpdateTelegramChatIdAsync(int userId, long newTelegramChatId);
    Task UpdateTelegramAsync(int userId, string newTelegram);
    Task UpdateEmailAsync(int userId, string newEmail);
    Task<string[]> GetUserSubscriptionsAsync(int userId);
}