using NotificationService.Domain.Entities;
using NotificationService.MessageBrokerAccess.Entities;

namespace NotificationService.Domain.Abstractions.BrokersServices;

public interface ITelegramService
{
    void StartListeningForResponses();
    Task SendEvent(Event message);
    Task SendUserForRegistration(User user);
}