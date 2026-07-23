namespace ProtectBroker.Core.Entities;

/// <summary>
/// Automation rule for triggering actions based on conditions
/// </summary>
public class Rule
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>JSON representation of rule conditions</summary>
    public string ConditionsJson { get; set; } = string.Empty;
    
    /// <summary>JSON representation of rule actions</summary>
    public string ActionsJson { get; set; } = string.Empty;
    
    /// <summary>If true, rule must be manually triggered</summary>
    public bool IsManualOnly { get; set; } = false;
    
    public int ExecutionCount { get; set; }
    
    public DateTime? LastExecuted { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastModifiedAt { get; set; }
}

/// <summary>
/// Execution history for audit and debugging
/// </summary>
public class RuleExecution
{
    public Guid Id { get; set; }
    
    public Guid RuleId { get; set; }
    
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    
    public RuleExecutionStatus Status { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    /// <summary>JSON log of what happened during execution</summary>
    public string? ExecutionLog { get; set; }
}

public enum RuleExecutionStatus
{
    Success,
    PartialSuccess,
    Failed
}
