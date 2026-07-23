namespace ProtectBroker.Core.Entities;

/// <summary>
/// Represents a farm zone or area for grouping related devices
/// </summary>
public class Zone
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    /// <summary>Icon identifier for UI display (barn, shed, field, etc.)</summary>
    public string? IconName { get; set; }
    
    public int SortOrder { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public List<Device> Devices { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastModifiedAt { get; set; }
}
