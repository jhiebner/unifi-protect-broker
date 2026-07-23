using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace ProtectBroker.Api.Hubs;

/// <summary>
/// SignalR hub for real-time device updates.
/// Broadcasts sensor readings, relay state changes, and device status updates.
/// </summary>
public class DeviceHub : Hub
{
    private readonly ILogger _logger = Log.ForContext<DeviceHub>();
    private const string DeviceSensorGroup = "devices-sensors";
    private const string DeviceRelayGroup = "devices-relays";
    private const string SystemGroup = "system";

    public override async Task OnConnectedAsync()
    {
        _logger.Information("Client connected: {ConnectionId}", Context.ConnectionId);
        
        // Add to default groups
        await Groups.AddToGroupAsync(Context.ConnectionId, DeviceSensorGroup);
        await Groups.AddToGroupAsync(Context.ConnectionId, DeviceRelayGroup);
        await Groups.AddToGroupAsync(Context.ConnectionId, SystemGroup);
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.Information("Client disconnected: {ConnectionId}", Context.ConnectionId);
        
        if (exception != null)
        {
            _logger.Warning(exception, "Client disconnected with exception");
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Broadcast sensor value update to all connected clients.
    /// </summary>
    public async Task BroadcastSensorUpdate(Guid deviceId, string sensorType, double currentValue, 
        double? batteryLevel, int? signalStrength, DateTime lastUpdate)
    {
        var update = new
        {
            deviceId,
            sensorType,
            currentValue,
            batteryLevel,
            signalStrength,
            lastUpdate,
            timestamp = DateTime.UtcNow
        };

        _logger.Debug("Broadcasting sensor update: {Update}", update);
        
        await Clients.Group(DeviceSensorGroup).SendAsync("SensorUpdated", update);
    }

    /// <summary>
    /// Broadcast relay state change to all connected clients.
    /// </summary>
    public async Task BroadcastRelayStateChange(Guid relayId, Guid deviceId, string state, string? reason = null)
    {
        var update = new
        {
            relayId,
            deviceId,
            state,
            reason,
            timestamp = DateTime.UtcNow
        };

        _logger.Debug("Broadcasting relay state change: {Update}", update);
        
        await Clients.Group(DeviceRelayGroup).SendAsync("RelayStateChanged", update);
    }

    /// <summary>
    /// Broadcast device online/offline status change.
    /// </summary>
    public async Task BroadcastDeviceStatusChange(Guid deviceId, string deviceName, string status)
    {
        var update = new
        {
            deviceId,
            deviceName,
            status,
            timestamp = DateTime.UtcNow
        };

        _logger.Debug("Broadcasting device status change: {Update}", update);
        
        await Clients.Group(SystemGroup).SendAsync("DeviceStatusChanged", update);
    }

    /// <summary>
    /// Broadcast system alert to all connected clients.
    /// </summary>
    public async Task BroadcastSystemAlert(string severity, string message, Guid? deviceId = null)
    {
        var alert = new
        {
            severity,
            message,
            deviceId,
            timestamp = DateTime.UtcNow
        };

        _logger.Warning("Broadcasting system alert [{Severity}]: {Message}", severity, message);
        
        await Clients.Group(SystemGroup).SendAsync("SystemAlert", alert);
    }

    /// <summary>
    /// Broadcast activity log entry to all connected clients.
    /// </summary>
    public async Task BroadcastActivityLog(string userId, string action, string entityType, 
        Guid? entityId, string? details = null)
    {
        var logEntry = new
        {
            userId,
            action,
            entityType,
            entityId,
            details,
            timestamp = DateTime.UtcNow
        };

        _logger.Debug("Broadcasting activity log: {LogEntry}", logEntry);
        
        await Clients.Group(SystemGroup).SendAsync("ActivityLogged", logEntry);
    }

    /// <summary>
    /// Broadcast UniFi Protect connection status.
    /// </summary>
    public async Task BroadcastProtectConnectionStatus(bool isConnected, string? message = null)
    {
        var status = new
        {
            isConnected,
            message,
            timestamp = DateTime.UtcNow
        };

        _logger.Information("Broadcasting Protect connection status: {Status}", isConnected ? "Connected" : "Disconnected");
        
        await Clients.Group(SystemGroup).SendAsync("ProtectConnectionStatusChanged", status);
    }

    /// <summary>
    /// Broadcast broker connection status.
    /// </summary>
    public async Task BroadcastBrokerHealthStatus(bool isHealthy, string? message = null)
    {
        var status = new
        {
            isHealthy,
            message,
            timestamp = DateTime.UtcNow
        };

        _logger.Information("Broadcasting broker health status: {Status}", isHealthy ? "Healthy" : "Unhealthy");
        
        await Clients.Group(SystemGroup).SendAsync("BrokerHealthStatusChanged", status);
    }

    /// <summary>
    /// Subscribe client to updates for a specific device.
    /// </summary>
    public async Task SubscribeToDevice(Guid deviceId)
    {
        var groupName = $"device-{deviceId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.Debug("Client subscribed to device: {DeviceId}", deviceId);
    }

    /// <summary>
    /// Unsubscribe client from a specific device.
    /// </summary>
    public async Task UnsubscribeFromDevice(Guid deviceId)
    {
        var groupName = $"device-{deviceId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.Debug("Client unsubscribed from device: {DeviceId}", deviceId);
    }

    /// <summary>
    /// Broadcast device-specific update to subscribers.
    /// </summary>
    public async Task BroadcastDeviceSpecificUpdate(Guid deviceId, string updateType, object updateData)
    {
        var groupName = $"device-{deviceId}";
        var update = new
        {
            deviceId,
            updateType,
            updateData,
            timestamp = DateTime.UtcNow
        };

        _logger.Debug("Broadcasting device-specific update: {DeviceId}", deviceId);
        
        await Clients.Group(groupName).SendAsync("DeviceUpdated", update);
    }
}
