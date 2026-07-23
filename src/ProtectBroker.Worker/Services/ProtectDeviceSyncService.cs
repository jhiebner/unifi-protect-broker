using Microsoft.Extensions.Hosting;
using Serilog;
using ProtectBroker.Core.Models;
using ProtectBroker.Infrastructure.Data;
using ProtectBroker.Infrastructure.Services;

namespace ProtectBroker.Worker.Services;

/// <summary>
/// Background service that periodically syncs devices from UniFi Protect.
/// Discovers cameras, sensors, and relays and stores them in database.
/// </summary>
public class ProtectDeviceSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger = Log.ForContext<ProtectDeviceSyncService>();
    
    private const int SyncIntervalSeconds = 300; // Sync every 5 minutes
    private DateTime _lastSyncTime = DateTime.MinValue;

    public ProtectDeviceSyncService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Information("Protect Device Sync Service started");

        try
        {
            // Initial sync on startup
            await PerformSyncAsync(stoppingToken);

            // Periodic sync
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(SyncIntervalSeconds), stoppingToken);

                try
                {
                    await PerformSyncAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error during device sync");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Information("Protect Device Sync Service cancelled");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error in Protect Device Sync Service");
        }
    }

    /// <summary>
    /// Perform a full device sync from UniFi Protect.
    /// </summary>
    private async Task PerformSyncAsync(CancellationToken cancellationToken)
    {
        if (_lastSyncTime.AddSeconds(SyncIntervalSeconds) > DateTime.UtcNow)
        {
            return; // Skip if recent sync
        }

        using var scope = _serviceProvider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IProtectApiClient>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            _logger.Information("Starting device sync from UniFi Protect");

            // Get bootstrap data
            var bootstrap = await client.GetBootstrapAsync(cancellationToken);

            if (bootstrap?.Cameras == null)
            {
                _logger.Warning("No cameras found in bootstrap response");
                return;
            }

            var syncResult = await SyncDevicesAsync(bootstrap, context, cancellationToken);

            _logger.Information(
                "Device sync completed: {CamerasAdded} cameras, {SensorsAdded} sensors added/updated",
                syncResult.CamerasCount,
                syncResult.SensorsCount);

            _lastSyncTime = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to sync devices from UniFi Protect");
        }
    }

    /// <summary>
    /// Sync all devices from bootstrap into database.
    /// </summary>
    private async Task<SyncResult> SyncDevicesAsync(
        BootstrapResponse bootstrap,
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var result = new SyncResult();

        try
        {
            // Sync cameras as devices
            if (bootstrap.Cameras != null)
            {
                foreach (var camera in bootstrap.Cameras)
                {
                    await SyncCameraAsync(camera, context);
                    result.CamerasCount++;
                }
            }

            // Sync sensors as devices
            if (bootstrap.Sensors != null)
            {
                foreach (var sensor in bootstrap.Sensors)
                {
                    await SyncSensorAsync(sensor, context);
                    result.SensorsCount++;
                }
            }

            // Save all changes
            await context.SaveChangesAsync(cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error syncing devices");
            throw;
        }
    }

    /// <summary>
    /// Sync a camera device.
    /// </summary>
    private async Task SyncCameraAsync(Camera camera, ApplicationDbContext context)
    {
        var deviceId = Guid.Parse(camera.Id ?? Guid.NewGuid().ToString());

        var existingDevice = await context.Devices.FindAsync(deviceId);

        if (existingDevice == null)
        {
            var newDevice = new Device
            {
                Id = deviceId,
                Name = camera.Name ?? "Unknown Camera",
                Type = "Camera",
                MacAddress = camera.Mac,
                IpAddress = camera.IpAddress,
                Status = "Online",
                Firmware = camera.FirmwareVersion,
                LastSeen = DateTime.UtcNow,
                ExternalDeviceId = camera.Id
            };

            context.Devices.Add(newDevice);
            _logger.Debug("Added new camera device: {Name} ({Id})", newDevice.Name, newDevice.Id);
        }
        else
        {
            // Update existing device
            existingDevice.Name = camera.Name ?? existingDevice.Name;
            existingDevice.IpAddress = camera.IpAddress ?? existingDevice.IpAddress;
            existingDevice.Status = camera.IsConnected ? "Online" : "Offline";
            existingDevice.Firmware = camera.FirmwareVersion ?? existingDevice.Firmware;
            existingDevice.LastSeen = DateTime.UtcNow;

            context.Devices.Update(existingDevice);
            _logger.Debug("Updated camera device: {Name}", existingDevice.Name);
        }
    }

    /// <summary>
    /// Sync a sensor device with readings.
    /// </summary>
    private async Task SyncSensorAsync(Sensor sensor, ApplicationDbContext context)
    {
        var deviceId = Guid.Parse(sensor.Id ?? Guid.NewGuid().ToString());

        var existingDevice = await context.Devices.FindAsync(deviceId);

        if (existingDevice == null)
        {
            var newDevice = new Device
            {
                Id = deviceId,
                Name = sensor.Name ?? "Unknown Sensor",
                Type = sensor.SensorType ?? "Unknown",
                MacAddress = sensor.Mac,
                IpAddress = sensor.IpAddress,
                Status = "Online",
                Firmware = sensor.FirmwareVersion,
                LastSeen = DateTime.UtcNow,
                Battery = sensor.Battery?.Percentage,
                ExternalDeviceId = sensor.Id
            };

            context.Devices.Add(newDevice);
            _logger.Debug("Added new sensor device: {Name} ({Id})", newDevice.Name, newDevice.Id);
        }
        else
        {
            // Update existing device
            existingDevice.Name = sensor.Name ?? existingDevice.Name;
            existingDevice.Type = sensor.SensorType ?? existingDevice.Type;
            existingDevice.IpAddress = sensor.IpAddress ?? existingDevice.IpAddress;
            existingDevice.Status = sensor.IsConnected ? "Online" : "Offline";
            existingDevice.Firmware = sensor.FirmwareVersion ?? existingDevice.Firmware;
            existingDevice.Battery = sensor.Battery?.Percentage ?? existingDevice.Battery;
            existingDevice.LastSeen = DateTime.UtcNow;

            context.Devices.Update(existingDevice);
            _logger.Debug("Updated sensor device: {Name}", existingDevice.Name);
        }

        // Sync current sensor reading
        if (sensor.SensorValues != null && sensor.SensorValues.Count > 0)
        {
            var reading = sensor.SensorValues.FirstOrDefault();
            if (reading != null)
            {
                var sensorRecord = (await context.Sensors.FindAsync(deviceId)) ?? new Core.Models.Sensor
                {
                    Id = deviceId,
                    DeviceId = deviceId,
                    SensorType = sensor.SensorType ?? "Unknown"
                };

                sensorRecord.CurrentValue = reading.Value;
                sensorRecord.LastUpdate = DateTime.UtcNow;
                sensorRecord.BatteryLevel = sensor.Battery?.Percentage;
                sensorRecord.SignalStrength = sensor.SignalStrength;

                if (sensorRecord.Id == Guid.Empty)
                {
                    sensorRecord.Id = deviceId;
                    context.Sensors.Add(sensorRecord);
                }
                else
                {
                    context.Sensors.Update(sensorRecord);
                }

                _logger.Debug("Synced sensor reading: {Name} = {Value}", sensorRecord.SensorType, reading.Value);
            }
        }
    }

    /// <summary>
    /// Result of a device sync operation.
    /// </summary>
    private class SyncResult
    {
        public int CamerasCount { get; set; }
        public int SensorsCount { get; set; }
    }
}
