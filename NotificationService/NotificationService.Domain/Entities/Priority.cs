﻿namespace NotificationService.Domain.Entities;

public class Priority
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    
    public ICollection<MessageTracking> MessageTrackings { get; set; }
}