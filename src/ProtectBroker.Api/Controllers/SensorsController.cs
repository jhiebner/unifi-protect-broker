using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtectBroker.Infrastructure.Data;

namespace ProtectBroker.Api.Controllers;

/// <summary>
/// API endpoints for sensor data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SensorsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SensorsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all sensors.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetSensors([FromQuery] string? type = null, [FromQuery] string? status = null)
    {
        var query = _context.Sensors.AsQueryable();

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(s => s.SensorType == type);
        }

        var sensors = await query.ToListAsync();

        // Filter by device status if specified
        if (!string.IsNullOrEmpty(status))
        {
            var deviceIds = await _context.Devices
                .Where(d => d.Status == status)
                .Select(d => d.Id)
                .ToListAsync();

            sensors = sensors.Where(s => deviceIds.Contains(s.DeviceId)).ToList();
        }

        return Ok(sensors);
    }

    /// <summary>
    /// Get a specific sensor by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetSensor(Guid id)
    {
        var sensor = await _context.Sensors.FindAsync(id);

        if (sensor == null)
        {
            return NotFound();
        }

        return Ok(sensor);
    }

    /// <summary>
    /// Get sensors by device ID.
    /// </summary>
    [HttpGet("device/{deviceId}")]
    public async Task<ActionResult> GetSensorsByDevice(Guid deviceId)
    {
        var sensors = await _context.Sensors
            .Where(s => s.DeviceId == deviceId)
            .ToListAsync();

        return Ok(sensors);
    }

    /// <summary>
    /// Get sensor readings history.
    /// </summary>
    [HttpGet("{id}/history")]
    public async Task<ActionResult> GetSensorHistory(Guid id, [FromQuery] int limit = 100)
    {
        var sensor = await _context.Sensors.FindAsync(id);

        if (sensor == null)
        {
            return NotFound();
        }

        // TODO: Implement sensor history retrieval from SensorReadings table
        return Ok(new { message = "History endpoint coming in Phase 3" });
    }
}
