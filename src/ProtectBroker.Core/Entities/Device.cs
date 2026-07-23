namespace ProtectBroker.Core.Entities;

/// <summary>
/// Represents a UniFi Protect device (camera, sensor, lock, etc.)
/// </summary>
public class Device
{
    public Guid Id { get; set; }
    
    /// <summary>UniFi Protect device ID</summary>
    public string ProtectDeviceId { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public DeviceType Type { get; set; }
    
    public string? MacAddress { get; set; }
    
    public string? IpAddress { get; set; }
    
    public DeviceStatus Status { get; set; } = DeviceStatus.Offline;
    
    /// <summary>Battery level percentage (0-100), null if wired</summary>
    public int? BatteryPercentage { get; set; }
    
    public string? FirmwareVersion { get; set; }
    
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    
    public string? Location { get; set; }
    
    public Guid? ZoneId { get; set; }
    
    public Zone? Zone { get; set; }
    
    public List<Sensor> Sensors { get; set; } = new();
    
    public List<Relay> Relays { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastModifiedAt { get; set; }
}

public enum DeviceType
{
    Camera,
    MotionSensor,
    DoorSensor,
    TemperatureSensor,
    HumiditySensor,
    LightSensor,
    WaterSensor,
    AlarmSensor,
    SmartLock,
    Relay,
    Other
}

public enum DeviceStatus
{
    Online,
    Offline,
    Disabled,
    Error
}
