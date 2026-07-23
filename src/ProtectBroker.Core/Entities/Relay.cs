namespace ProtectBroker.Core.Entities;

/// <summary>
/// Represents a relay control (typically from UniFi Smart Lock)
/// </summary>
public class Relay
{
    public Guid Id { get; set; }
    
    /// <summary>UniFi Protect relay ID</summary>
    public string ProtectRelayId { get; set; } = string.Empty;
    
    public Guid DeviceId { get; set; }
    
    public Device? Device { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public RelayState CurrentState { get; set; } = RelayState.Off;
    
    /// <summary>Default pulse duration in milliseconds</summary>
    public int DefaultPulseDurationMs { get; set; } = 500;
    
    /// <summary>Whether to require confirmation before toggling</summary>
    public bool RequiresConfirmation { get; set; } = true;
    
    /// <summary>When enabled, prevents relay changes outside this window (UTC)</summary>
    public TimeOnly? SafetyLockoutStartUtc { get; set; }
    
    public TimeOnly? SafetyLockoutEndUtc { get; set; }
    
    public DateTime LastToggled { get; set; } = DateTime.UtcNow;
    
    public Guid? LastToggledByUserId { get; set; }
    
    public List<RelayCommand> Commands { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastModifiedAt { get; set; }
}

public enum RelayState
{
    On,
    Off,
    Pulsing
}

/// <summary>
/// Audit trail for relay commands
/// </summary>
public class RelayCommand
{
    public Guid Id { get; set; }
    
    public Guid RelayId { get; set; }
    
    public Relay? Relay { get; set; }
    
    public Guid? RequestedByUserId { get; set; }
    
    public RelayCommandType CommandType { get; set; }
    
    /// <summary>For pulse commands, duration in milliseconds</summary>
    public int? PulseDurationMs { get; set; }
    
    public RelayCommandStatus Status { get; set; } = RelayCommandStatus.Pending;
    
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExecutedAt { get; set; }
    
    public string? ErrorMessage { get; set; }
}

public enum RelayCommandType
{
    TurnOn,
    TurnOff,
    Toggle,
    Pulse
}

public enum RelayCommandStatus
{
    Pending,
    Executing,
    Success,
    Failed
}
