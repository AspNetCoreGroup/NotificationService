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

    public async Task AddUserAsync(User? user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User? user)
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

    public async Task AddSubscriptionToUserAsync(int userId, string subscriptionTypeCode)
    {
        var subscriptionType = await _context.SubscriptionTypes
            .FirstOrDefaultAsync(mds => mds.Code == subscriptionTypeCode);

        if (subscriptionType == null)
        {
            throw new ArgumentException($"Invalid MessageDeliveryStatusCode: {subscriptionTypeCode}");
        }
        
        var user = await GetUserByIdAsync(userId);
        if (user != null)
        {
            var subscription = new UserSubscription
            {
                UserId = userId,
                SubscriptionTypeId = subscriptionType.Id
            };
            _context.UserSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task RemoveSubscriptionFromUserAsync(int userId, string subscriptionTypeCode)
    {
        var subscriptionType = await _context.SubscriptionTypes
            .FirstOrDefaultAsync(mds => mds.Code == subscriptionTypeCode);

        if (subscriptionType == null)
        {
            throw new ArgumentException($"Invalid MessageDeliveryStatusCode: {subscriptionTypeCode}");
        }
        
        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(us => us.UserId == userId && us.SubscriptionTypeId == subscriptionType.Id);
        if (subscription != null)
        {
            _context.UserSubscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateTelegramChatIdAsync(int userId, long newTelegramChatId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user != null)
        {
            user.TelegramChatId = newTelegramChatId;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateTelegramAsync(int userId, string newTelegram)
    {
        var user = await GetUserByIdAsync(userId);
        if (user != null)
        {
            user.Telegram = newTelegram;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateEmailAsync(int userId, string newEmail)
    {
        var user = await GetUserByIdAsync(userId);
        if (user != null)
        {
            user.Email = newEmail;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<string[]> GetUserSubscriptionsAsync(int userId)
    {
        return await _context.UserSubscriptions
            .Where(us => us.UserId == userId)
            .Select(us => us.SubscriptionType.Code)
            .ToArrayAsync();
    }
}