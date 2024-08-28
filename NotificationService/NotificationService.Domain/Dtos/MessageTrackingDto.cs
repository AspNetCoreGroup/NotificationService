namespace NotificationService.Domain.Dtos;

public class MessageTrackingDto
{
    public string MessageDeliveryStatusCode { get; set; } = string.Empty;
    public string SubscriptionTypeCode { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public int UserId { get; set; }
}