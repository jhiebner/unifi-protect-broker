using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProtectBroker.Infrastructure.Services;

namespace ProtectBroker.Api.Controllers;

/// <summary>
/// Debug endpoints for development and troubleshooting.
/// Only available in Development mode.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DebugController : ControllerBase
{
    private readonly IProtectApiClient _protectClient;
    private readonly IProtectWebSocketClient _webSocketClient;
    private readonly IWebHostEnvironment _environment;

    public DebugController(
        IProtectApiClient protectClient,
        IProtectWebSocketClient webSocketClient,
        IWebHostEnvironment environment)
    {
        _protectClient = protectClient;
        _webSocketClient = webSocketClient;
        _environment = environment;
    }

    /// <summary>
    /// Test connection to UniFi Protect.
    /// </summary>
    [HttpGet("protect-status")]
    public async Task<ActionResult> GetProtectStatus()
    {
        if (!_environment.IsDevelopment())
        {
            return Forbid();
        }

        try
        {
            var isConnected = await _protectClient.IsConnectedAsync();
            var wsConnected = await _webSocketClient.IsConnectedAsync();

            return Ok(new
            {
                apiConnected = isConnected,
                webSocketConnected = wsConnected,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get bootstrap data from Protect (devices, cameras, sensors).
    /// </summary>
    [HttpGet("protect-bootstrap")]
    public async Task<ActionResult> GetBootstrap()
    {
        if (!_environment.IsDevelopment())
        {
            return Forbid();
        }

        try
        {
            var bootstrap = await _protectClient.GetBootstrapAsync();

            if (bootstrap == null)
            {
                return StatusCode(500, new { error = "Failed to get bootstrap data" });
            }

            return Ok(new
            {
                cameras = bootstrap.Cameras?.Count ?? 0,
                sensors = bootstrap.Sensors?.Count ?? 0,
                nvr = bootstrap.Nvr != null ? new { name = bootstrap.Nvr.Name } : null,
                bridge = bootstrap.Bridge != null ? new { name = bootstrap.Bridge.Name } : null,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get detailed bootstrap data (useful for debugging).
    /// </summary>
    [HttpGet("protect-bootstrap-detailed")]
    public async Task<ActionResult> GetBootstrapDetailed()
    {
        if (!_environment.IsDevelopment())
        {
            return Forbid();
        }

        try
        {
            var bootstrap = await _protectClient.GetBootstrapAsync();
            return Ok(bootstrap);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// WebSocket connection status.
    /// </summary>
    [HttpGet("websocket-status")]
    public async Task<ActionResult> GetWebSocketStatus()
    {
        if (!_environment.IsDevelopment())
        {
            return Forbid();
        }

        var isConnected = await _webSocketClient.IsConnectedAsync();
        return Ok(new
        {
            connected = isConnected,
            timestamp = DateTime.UtcNow
        });
    }
}
