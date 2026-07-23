namespace ProtectBroker.Core.Entities;

/// <summary>
/// Application settings and configuration stored in database
/// </summary>
public class Setting
{
    public Guid Id { get; set; }
    
    /// <summary>Setting key (e.g., "unifi.host", "smtp.port")</summary>
    public string Key { get; set; } = string.Empty;
    
    public string Value { get; set; } = string.Empty;
    
    /// <summary>Category for UI organization</summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>User-friendly description</summary>
    public string? Description { get; set; }
    
    /// <summary>If true, value is encrypted in database</summary>
    public bool IsEncrypted { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastModifiedAt { get; set; }
}

/// <summary>
/// Notification event template
/// </summary>
public class Notification
{
    public Guid Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string Message { get; set; } = string.Empty;
    
    public NotificationType Type { get; set; }
    
    public NotificationChannel[] Channels { get; set; } = Array.Empty<NotificationChannel>();
    
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? SentAt { get; set; }
    
    public string? ErrorMessage { get; set; }
}

public enum NotificationType
{
    Alert,
    Warning,
    Info,
    Error
}

public enum NotificationChannel
{
    Email,
    Sms,
    Push,
    Webhook,
    Discord,
    MicrosoftTeams,
    Slack
}

public enum NotificationStatus
{
    Pending,
    Sending,
    Sent,
    Failed,
    Retrying
}
