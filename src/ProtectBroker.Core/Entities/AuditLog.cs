namespace ProtectBroker.Core.Entities;

/// <summary>
/// Comprehensive audit logging for all system actions
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    
    public Guid? UserId { get; set; }
    
    public string Username { get; set; } = string.Empty;
    
    public AuditActionType ActionType { get; set; }
    
    public string EntityType { get; set; } = string.Empty;
    
    public Guid? EntityId { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    /// <summary>JSON representation of changes (old vs new values)</summary>
    public string? Changes { get; set; }
    
    public string? IpAddress { get; set; }
    
    public AuditSeverity Severity { get; set; } = AuditSeverity.Info;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public enum AuditActionType
{
    Login,
    Logout,
    Create,
    Read,
    Update,
    Delete,
    RelayCommand,
    RuleExecution,
    ConfigurationChange,
    SecurityEvent
}

public enum AuditSeverity
{
    Info,
    Warning,
    Error,
    Critical
}
