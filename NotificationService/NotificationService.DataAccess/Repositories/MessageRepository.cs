using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Abstractions.Repositories;
using NotificationService.Domain.Entities;

namespace NotificationService.DataAccess.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly DataContext _context;

    public MessageRepository(DataContext context)
    {
        _context = context;
    }
    
    public async Task<MessageTracking?> GetMessageByIdAsync(long id)
    {
        return await _context.MessageTrackings
            .Include(mt => mt.User)
            .Include(mt => mt.MessageDeliveryStatus)
            .Include(mt => mt.SubscriptionType)
            .FirstOrDefaultAsync(mt => mt.MessageId == id);
    }

    public async Task<IEnumerable<MessageTracking>> GetAllMessagesAsync()
    {
        return await _context.MessageTrackings
            .Include(mt => mt.User)
            .Include(mt => mt.MessageDeliveryStatus)
            .Include(mt => mt.SubscriptionType)
            .ToListAsync();
    }

    public async Task AddMessageAsync(MessageTracking messageTracking)
    {
        _context.MessageTrackings.Add(messageTracking);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateMessageAsync(MessageTracking messageTracking)
    {
        _context.MessageTrackings.Update(messageTracking);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteMessageAsync(long id)
    {
        var messageTracking = await GetMessageByIdAsync(id);
        if (messageTracking != null)
        {
            _context.MessageTrackings.Remove(messageTracking);
            await _context.SaveChangesAsync();
        }
    }
}