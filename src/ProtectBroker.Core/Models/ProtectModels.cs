using System.Text.Json.Serialization;

namespace ProtectBroker.Core.Models;

/// <summary>
/// Models for UniFi Protect API communication
/// </summary>

// Bootstrap response from Protect
public class BootstrapResponse
{
    [JsonPropertyName("authUserId")]
    public string AuthUserId { get; set; } = string.Empty;
    
    [JsonPropertyName("bridges")]
    public Bridge[]? Bridges { get; set; }
    
    [JsonPropertyName("cameras")]
    public Camera[]? Cameras { get; set; }
    
    [JsonPropertyName("sensors")]
    public Sensor[]? Sensors { get; set; }
    
    [JsonPropertyName("liveviews")]
    public LiveView[]? Liveviews { get; set; }
    
    [JsonPropertyName("nvr")]
    public Nvr? Nvr { get; set; }
    
    [JsonPropertyName("events")]
    public Event[]? Events { get; set; }
}

// UniFi Protect Bridge
public class Bridge
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("modelKey")]
    public string ModelKey { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("ispVersion")]
    public string? IspVersion { get; set; }
    
    [JsonPropertyName("firmwareVersion")]
    public string? FirmwareVersion { get; set; }
    
    [JsonPropertyName("mac")]
    public string? Mac { get; set; }
    
    [JsonPropertyName("uptime")]
    public long Uptime { get; set; }
}

// NVR (Network Video Recorder)
public class Nvr
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("host")]
    public string Host { get; set; } = string.Empty;
    
    [JsonPropertyName("port")]
    public int Port { get; set; }
    
    [JsonPropertyName("firmware")]
    public string? Firmware { get; set; }
    
    [JsonPropertyName("mac")]
    public string? Mac { get; set; }
    
    [JsonPropertyName("uptime")]
    public long Uptime { get; set; }
}

// Camera device
public class Camera
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("modelKey")]
    public string ModelKey { get; set; } = string.Empty;
    
    [JsonPropertyName("mac")]
    public string? Mac { get; set; }
    
    [JsonPropertyName("ip")]
    public string? Ip { get; set; }
    
    [JsonPropertyName("connected")]
    public bool Connected { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    [JsonPropertyName("battery")]
    public Battery? Battery { get; set; }
    
    [JsonPropertyName("firmwareVersion")]
    public string? FirmwareVersion { get; set; }
}

// Sensor device
public class Sensor
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("modelKey")]
    public string ModelKey { get; set; } = string.Empty;
    
    [JsonPropertyName("mac")]
    public string? Mac { get; set; }
    
    [JsonPropertyName("connected")]
    public bool Connected { get; set; }
    
    [JsonPropertyName("battery")]
    public Battery? Battery { get; set; }
    
    [JsonPropertyName("temperature")]
    public SensorValue? Temperature { get; set; }
    
    [JsonPropertyName("humidity")]
    public SensorValue? Humidity { get; set; }
    
    [JsonPropertyName("lastMotionTime")]
    public long LastMotionTime { get; set; }
    
    [JsonPropertyName("firmwareVersion")]
    public string? FirmwareVersion { get; set; }
}

// Battery information
public class Battery
{
    [JsonPropertyName("isLow")]
    public bool IsLow { get; set; }
    
    [JsonPropertyName("percentage")]
    public int Percentage { get; set; }
}

// Sensor reading value
public class SensorValue
{
    [JsonPropertyName("value")]
    public decimal Value { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

// Live view
public class LiveView
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("cameras")]
    public string[]? Cameras { get; set; }
}

// Event from Protect
public class Event
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("camera")]
    public string? Camera { get; set; }
    
    [JsonPropertyName("smartDetectTypes")]
    public string[]? SmartDetectTypes { get; set; }
    
    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }
    
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}

// Authentication response
public class AuthResponse
{
    [JsonPropertyName("accessKey")]
    public string AccessKey { get; set; } = string.Empty;
    
    [JsonPropertyName("secretKey")]
    public string SecretKey { get; set; } = string.Empty;
    
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
}
