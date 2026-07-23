namespace ProtectBroker.Core.Entities;

/// <summary>
/// Represents a sensor reading from a device (motion, door, temperature, etc.)
/// </summary>
public class Sensor
{
    public Guid Id { get; set; }
    
    public Guid DeviceId { get; set; }
    
    public Device? Device { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public SensorType Type { get; set; }
    
    /// <summary>Current sensor reading value (JSON string for flexibility)</summary>
    public string CurrentValue { get; set; } = string.Empty;
    
    /// <summary>Unit of measurement (C, F, %, Lux, etc.)</summary>
    public string? Unit { get; set; }
    
    public int? SignalStrength { get; set; }
    
    public int? BatteryPercentage { get; set; }
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    public List<SensorReading> Readings { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum SensorType
{
    Motion,
    DoorContact,
    Temperature,
    Humidity,
    Light,
    Water,
    Alarm,
    Custom
}

/// <summary>
/// Historical sensor reading for trend analysis
/// </summary>
public class SensorReading
{
    public Guid Id { get; set; }
    
    public Guid SensorId { get; set; }
    
    public Sensor? Sensor { get; set; }
    
    public string Value { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
