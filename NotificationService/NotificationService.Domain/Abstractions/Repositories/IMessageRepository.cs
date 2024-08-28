using NotificationService.Domain.Dtos;
using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Abstractions.Repositories;

public interface IMessageRepository
{
    Task<MessageTracking?> GetMessageByIdAsync(long id);
    Task<IEnumerable<MessageTracking>> GetAllMessagesAsync();
    Task<long> AddMessageAsync(MessageTrackingDto messageTrackingDto);
    Task UpdateMessageAsync(MessageTracking messageTracking);
    Task DeleteMessageAsync(long id);
    Task UpdateMessageDeliveryStatusAsync(long messageId, string newStatusCode);
    Task IncrementRetryCountAsync(long messageId);
}