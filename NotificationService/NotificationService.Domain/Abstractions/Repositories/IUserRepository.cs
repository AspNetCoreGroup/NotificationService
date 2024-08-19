using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Abstractions.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(int id);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int id);
    Task AddSubscriptionToUserAsync(int userId, int subscriptionTypeId);
}