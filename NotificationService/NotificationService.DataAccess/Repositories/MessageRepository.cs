using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Abstractions.Repositories;
using NotificationService.Domain.Dtos;
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

    public async Task<long> AddMessageAsync(MessageTrackingDto messageTrackingDto)
    {
        var messageDeliveryStatus = await _context.MessageDeliveryStatuses
            .FirstOrDefaultAsync(mds => mds.Code == messageTrackingDto.MessageDeliveryStatusCode);

        if (messageDeliveryStatus == null)
        {
            throw new ArgumentException($"Invalid MessageDeliveryStatusCode: {messageTrackingDto.MessageDeliveryStatusCode}");
        }

        var subscriptionType = await _context.SubscriptionTypes
            .FirstOrDefaultAsync(st => st.Code == messageTrackingDto.SubscriptionTypeCode);

        if (subscriptionType == null)
        {
            throw new ArgumentException($"Invalid SubscriptionTypeCode: {messageTrackingDto.SubscriptionTypeCode}");
        }

        var messageTracking = new MessageTracking
        {
            MessageDeliveryStatusId = messageDeliveryStatus.Id,
            SubscriptionTypeId = subscriptionType.Id,
            MessageText = messageTrackingDto.MessageText,
            UserId = messageTrackingDto.UserId,
            CreationDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            RetryCount = 0
        };

        var result = _context.MessageTrackings.Add(messageTracking);
        await _context.SaveChangesAsync();

        return result.Entity.MessageId;
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
    
    public async Task UpdateMessageDeliveryStatusAsync(long messageId, string newStatusCode)
    {
        var message = await GetMessageByIdAsync(messageId);
        if (message != null)
        {
            var status = await _context.MessageDeliveryStatuses
                .FirstOrDefaultAsync(mds => mds.Code == newStatusCode);

            if (status != null)
            {
                message.MessageDeliveryStatusId = status.Id;
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task IncrementRetryCountAsync(long messageId)
    {
        var message = await GetMessageByIdAsync(messageId);
        if (message != null)
        {
            message.RetryCount++;
            await _context.SaveChangesAsync();
        }
    }
}