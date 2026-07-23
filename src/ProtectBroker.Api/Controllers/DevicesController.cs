using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtectBroker.Infrastructure.Data;

namespace ProtectBroker.Api.Controllers;

/// <summary>
/// API endpoints for device management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DevicesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all devices.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetDevices([FromQuery] string? status = null, [FromQuery] string? type = null)
    {
        var query = _context.Devices.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(d => d.Status == status);
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(d => d.Type == type);
        }

        var devices = await query.ToListAsync();
        return Ok(devices);
    }

    /// <summary>
    /// Get a specific device by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetDevice(Guid id)
    {
        var device = await _context.Devices.FindAsync(id);

        if (device == null)
        {
            return NotFound();
        }

        return Ok(device);
    }

    /// <summary>
    /// Get device by external ID (UniFi Protect device ID).
    /// </summary>
    [HttpGet("external/{externalId}")]
    public async Task<ActionResult> GetDeviceByExternalId(string externalId)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.ExternalDeviceId == externalId);

        if (device == null)
        {
            return NotFound();
        }

        return Ok(device);
    }
}
