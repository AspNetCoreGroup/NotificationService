using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Abstractions.Repositories;
using NotificationService.Domain.Entities;

namespace NotificationService.DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;

    public UserRepository(DataContext context)
    {
        _context = context;
    }
    
    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.UserSubscriptions)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.UserSubscriptions)
            .ToListAsync();
    }

    public async Task AddUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await GetUserByIdAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddSubscriptionToUserAsync(int userId, int subscriptionTypeId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user != null)
        {
            var subscription = new UserSubscription
            {
                UserId = userId,
                SubscriptionTypeId = subscriptionTypeId
            };
            _context.UserSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();
        }
    }
}