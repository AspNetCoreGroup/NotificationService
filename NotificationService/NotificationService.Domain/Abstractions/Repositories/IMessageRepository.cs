using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Abstractions.Repositories;

public interface IMessageRepository
{
    Task<MessageTracking?> GetMessageByIdAsync(long id);
    Task<IEnumerable<MessageTracking>> GetAllMessagesAsync();
    Task AddMessageAsync(MessageTracking messageTracking);
    Task UpdateMessageAsync(MessageTracking messageTracking);
    Task DeleteMessageAsync(long id);
}