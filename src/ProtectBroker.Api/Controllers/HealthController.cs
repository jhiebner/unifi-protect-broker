using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProtectBroker.Infrastructure.Data;

namespace ProtectBroker.Api.Controllers;

/// <summary>
/// Health check endpoints for monitoring application status.
/// </summary>
[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ApplicationDbContext dbContext, ILogger<HealthController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Get basic application health status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealth()
    {
        try
        {
            // Check database connectivity
            var canConnect = await _dbContext.Database.CanConnectAsync();
            if (!canConnect)
            {
                _logger.LogWarning("Database health check failed");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                    new { status = "unhealthy", reason = "Database unreachable" });
            }

            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0-alpha"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { status = "unhealthy", reason = ex.Message });
        }
    }

    /// <summary>
    /// Detailed health status including all components.
    /// </summary>
    [HttpGet("detailed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDetailedHealth()
    {
        var details = new Dictionary<string, object>
        {
            { "timestamp", DateTime.UtcNow },
            { "uptime", Environment.TickCount64 / 1000 },
            { "environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") }
        };

        try
        {
            details["database"] = await _dbContext.Database.CanConnectAsync() 
                ? "healthy" 
                : "unhealthy";
        }
        catch
        {
            details["database"] = "error";
        }

        return Ok(details);
    }
}
